[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $PlanPath,

    [Parameter(Mandatory)]
    [string] $PublishPath,

    [switch] $DryRun
)

$ErrorActionPreference = 'Stop'
$protectedRoots = @('mdb-database', 'logs', 'Portfolio')
$plan = Get-Content -Raw -LiteralPath $PlanPath | ConvertFrom-Json
$publishRoot = (Resolve-Path -LiteralPath $PublishPath).Path

function Assert-SafeRelativePath([string] $Path, [string] $Description) {
    if ([string]::IsNullOrWhiteSpace($Path) -or
        [IO.Path]::IsPathRooted($Path) -or
        $Path.Contains('..') -or
        $Path -notmatch '^[A-Za-z0-9._/\\-]+$') {
        throw "$Description is not a safe relative path: $Path"
    }

    $root = ($Path -split '[/\\]')[0]
    if ($protectedRoots -contains $root) {
        throw "$Description targets protected runtime data: $Path"
    }
}

function Resolve-PublishItem([string] $RelativePath, [string] $ExpectedType) {
    Assert-SafeRelativePath $RelativePath 'Publish source'
    $candidate = [IO.Path]::GetFullPath((Join-Path $publishRoot $RelativePath))
    $prefix = $publishRoot + [IO.Path]::DirectorySeparatorChar

    if (-not $candidate.StartsWith($prefix, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Publish source is outside the publish directory: $RelativePath"
    }

    if ($ExpectedType -eq 'File' -and -not (Test-Path -LiteralPath $candidate -PathType Leaf)) {
        throw "Publish file does not exist: $RelativePath"
    }

    if ($ExpectedType -eq 'Directory' -and -not (Test-Path -LiteralPath $candidate -PathType Container)) {
        throw "Publish directory does not exist: $RelativePath"
    }

    return $candidate
}

$uploads = [Collections.Generic.List[object]]::new()

foreach ($entry in @($plan.uploadFiles)) {
    Assert-SafeRelativePath $entry.destination 'Upload destination'
    $uploads.Add([pscustomobject]@{
        Source = Resolve-PublishItem $entry.source 'File'
        Destination = $entry.destination.Replace('\', '/')
    })
}

foreach ($entry in @($plan.uploadDirectories)) {
    Assert-SafeRelativePath $entry.destination 'Upload directory destination'
    $sourceDirectory = Resolve-PublishItem $entry.source 'Directory'

    foreach ($file in Get-ChildItem -LiteralPath $sourceDirectory -Recurse -File) {
        $relativeFile = [IO.Path]::GetRelativePath($sourceDirectory, $file.FullName).Replace('\', '/')
        $destination = ($entry.destination.TrimEnd('/', '\') + '/' + $relativeFile).TrimStart('/')
        Assert-SafeRelativePath $destination 'Expanded upload destination'
        $uploads.Add([pscustomobject]@{ Source = $file.FullName; Destination = $destination })
    }
}

$deletions = foreach ($path in @($plan.deleteFiles)) {
    Assert-SafeRelativePath $path 'Delete destination'
    $path.Replace('\', '/')
}

Write-Output "Deployment plan: $($plan.id)"
Write-Output "Files to upload: $($uploads.Count)"
$uploads | ForEach-Object { Write-Output "UPLOAD $($_.Destination)" }
Write-Output "Files to delete: $(@($deletions).Count)"
$deletions | ForEach-Object { Write-Output "DELETE $_" }

if ($DryRun) {
    Write-Output 'Dry run completed. No remote operation was performed.'
    exit 0
}

if ($plan.deployable -ne $true) {
    throw "Deployment plan is not explicitly marked as deployable: $($plan.id)"
}

$server = $env:ARUBA_FTP_SERVER
$username = $env:ARUBA_FTP_USERNAME
$password = $env:ARUBA_FTP_PASSWORD

if ([string]::IsNullOrWhiteSpace($server) -or
    [string]::IsNullOrWhiteSpace($username) -or
    [string]::IsNullOrWhiteSpace($password)) {
    throw 'ARUBA_FTP_SERVER, ARUBA_FTP_USERNAME and ARUBA_FTP_PASSWORD are required.'
}

$credential = [Net.NetworkCredential]::new($username, $password)

function New-FtpsRequest([string] $RemotePath, [string] $Method) {
    $escapedPath = ($RemotePath -split '/' | ForEach-Object { [Uri]::EscapeDataString($_) }) -join '/'
    $uri = "ftp://$server/$escapedPath"
    $request = [Net.FtpWebRequest]::Create($uri)
    $request.Method = $Method
    $request.Credentials = $credential
    $request.EnableSsl = $true
    $request.UsePassive = $true
    $request.KeepAlive = $false
    $request.Timeout = 30000
    $request.ReadWriteTimeout = 30000
    return $request
}

function Invoke-WithRetry([scriptblock] $Operation, [string] $Description) {
    foreach ($attempt in 1..5) {
        try {
            & $Operation
            return
        }
        catch {
            if ($attempt -ge 5) {
                throw "$Description failed after $attempt attempts. $($_.Exception.Message)"
            }

            Start-Sleep -Seconds $attempt
        }
    }
}

function Ensure-RemoteDirectory([string] $RemoteFile) {
    $segments = $RemoteFile.Split('/')
    if ($segments.Count -le 1) {
        return
    }

    $current = ''
    foreach ($segment in $segments[0..($segments.Count - 2)]) {
        $current = if ($current) { "$current/$segment" } else { $segment }
        try {
            $request = New-FtpsRequest $current ([Net.WebRequestMethods+Ftp]::MakeDirectory)
            $response = $request.GetResponse()
            $response.Dispose()
        }
        catch [Net.WebException] {
            if ($_.Exception.Response.StatusCode -ne [Net.FtpStatusCode]::ActionNotTakenFileUnavailable) {
                throw
            }
        }
    }
}

function Upload-Bytes([byte[]] $Content, [string] $RemotePath) {
    Ensure-RemoteDirectory $RemotePath
    $request = New-FtpsRequest $RemotePath ([Net.WebRequestMethods+Ftp]::UploadFile)
    $request.ContentLength = $Content.Length
    $stream = $request.GetRequestStream()
    try {
        $stream.Write($Content, 0, $Content.Length)
    }
    finally {
        $stream.Dispose()
    }

    $response = $request.GetResponse()
    $response.Dispose()
}

function Upload-File([string] $Source, [string] $RemotePath) {
    Upload-Bytes ([IO.File]::ReadAllBytes($Source)) $RemotePath
}

function Remove-RemoteFile([string] $RemotePath, [bool] $IgnoreMissing) {
    try {
        $request = New-FtpsRequest $RemotePath ([Net.WebRequestMethods+Ftp]::DeleteFile)
        $response = $request.GetResponse()
        $response.Dispose()
    }
    catch [Net.WebException] {
        if (-not $IgnoreMissing -or $_.Exception.Response.StatusCode -ne [Net.FtpStatusCode]::ActionNotTakenFileUnavailable) {
            throw
        }
    }
}

$offlineUploaded = $false
$remoteChanged = $false
$deploymentSucceeded = $false
try {
    $offlineContent = [Text.Encoding]::UTF8.GetBytes('<!doctype html><title>Manutenzione</title><h1>Aggiornamento in corso</h1>')
    Invoke-WithRetry { Upload-Bytes $offlineContent 'app_offline.htm' } 'Upload app_offline.htm'
    $offlineUploaded = $true
    Start-Sleep -Seconds 3

    foreach ($entry in $uploads) {
        Invoke-WithRetry { Upload-File $entry.Source $entry.Destination } "Upload $($entry.Destination)"
        $remoteChanged = $true
    }

    foreach ($remotePath in $deletions) {
        Invoke-WithRetry { Remove-RemoteFile $remotePath $true } "Delete $remotePath"
        $remoteChanged = $true
    }

    $deploymentSucceeded = $true
}
finally {
    if ($offlineUploaded -and ($deploymentSucceeded -or -not $remoteChanged)) {
        Invoke-WithRetry { Remove-RemoteFile 'app_offline.htm' $true } 'Remove app_offline.htm'
    }
    elseif ($offlineUploaded) {
        Write-Warning 'The deployment changed remote files before failing. app_offline.htm was intentionally preserved to avoid exposing a partial release.'
    }
}

Write-Output 'Targeted Aruba deployment completed.'
