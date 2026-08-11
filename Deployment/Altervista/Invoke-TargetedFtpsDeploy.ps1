[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $PlanPath,

    [Parameter(Mandatory)]
    [string] $SourcePath,

    [switch] $DryRun
)

$ErrorActionPreference = 'Stop'
$plan = Get-Content -Raw -LiteralPath $PlanPath | ConvertFrom-Json
$sourceRoot = (Resolve-Path -LiteralPath $SourcePath).Path
$forbiddenSourceRoots = @('bin', 'obj', 'DatabaseScripts', 'Properties')
$forbiddenSourceFiles = @('Portfolio.Web.csproj', 'Convert-ToUtf8NoBom.ps1')
$allowedLogInfrastructureFile = 'portfolio/internal/logs/.htaccess'

function Assert-SafeRelativePath([string] $Path, [string] $Description) {
    if ([string]::IsNullOrWhiteSpace($Path) -or
        [IO.Path]::IsPathRooted($Path) -or
        $Path.Contains('..') -or
        $Path -notmatch '^[A-Za-z0-9._/\-]+$') {
        throw "$Description is not a safe relative path: $Path"
    }
}

function Assert-DeployableWebPath([string] $Path, [string] $Description) {
    Assert-SafeRelativePath $Path $Description
    $normalized = $Path.Replace('\', '/').TrimStart('/')
    $root = ($normalized -split '/')[0]

    if ($forbiddenSourceRoots -contains $root -or $forbiddenSourceFiles -contains $normalized) {
        throw "$Description targets a development-only artifact: $Path"
    }

    if ($normalized.StartsWith('portfolio/internal/logs/', [StringComparison]::OrdinalIgnoreCase) -and
        $normalized -ne $allowedLogInfrastructureFile) {
        throw "$Description targets protected Portfolio.Web runtime logs: $Path"
    }
}

function Resolve-SourceFile([string] $RelativePath) {
    Assert-DeployableWebPath $RelativePath 'Upload path'
    $candidate = [IO.Path]::GetFullPath((Join-Path $sourceRoot $RelativePath))
    $prefix = $sourceRoot + [IO.Path]::DirectorySeparatorChar

    if (-not $candidate.StartsWith($prefix, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Upload source is outside the Portfolio.Web directory: $RelativePath"
    }

    if (-not (Test-Path -LiteralPath $candidate -PathType Leaf)) {
        throw "Upload file does not exist: $RelativePath"
    }

    return $candidate
}

$uploads = foreach ($path in @($plan.uploadFiles)) {
    $normalized = $path.Replace('\', '/').TrimStart('/')
    [pscustomobject]@{
        Source = Resolve-SourceFile $normalized
        Destination = $normalized
    }
}

$deletions = foreach ($path in @($plan.deleteFiles)) {
    Assert-DeployableWebPath $path 'Delete path'
    $path.Replace('\', '/').TrimStart('/')
}

Write-Output "Deployment plan: $($plan.id)"
Write-Output "Files to upload: $(@($uploads).Count)"
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

$server = $env:ALTERVISTA_FTP_SERVER
$username = $env:ALTERVISTA_FTP_USERNAME
$password = $env:ALTERVISTA_FTP_PASSWORD

if ([string]::IsNullOrWhiteSpace($server) -or
    [string]::IsNullOrWhiteSpace($username) -or
    [string]::IsNullOrWhiteSpace($password)) {
    throw 'ALTERVISTA_FTP_SERVER, ALTERVISTA_FTP_USERNAME and ALTERVISTA_FTP_PASSWORD are required.'
}

$credential = [Net.NetworkCredential]::new($username, $password)

function New-FtpsRequest([string] $RemotePath, [string] $Method) {
    $escapedPath = ($RemotePath -split '/' | ForEach-Object { [Uri]::EscapeDataString($_) }) -join '/'
    $request = [Net.FtpWebRequest]::Create("ftp://$server/$escapedPath")
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

function Upload-File([string] $Source, [string] $RemotePath) {
    Ensure-RemoteDirectory $RemotePath
    $content = [IO.File]::ReadAllBytes($Source)
    $request = New-FtpsRequest $RemotePath ([Net.WebRequestMethods+Ftp]::UploadFile)
    $request.ContentLength = $content.Length
    $stream = $request.GetRequestStream()
    try {
        $stream.Write($content, 0, $content.Length)
    }
    finally {
        $stream.Dispose()
    }

    $response = $request.GetResponse()
    $response.Dispose()
}

function Remove-RemoteFile([string] $RemotePath) {
    try {
        $request = New-FtpsRequest $RemotePath ([Net.WebRequestMethods+Ftp]::DeleteFile)
        $response = $request.GetResponse()
        $response.Dispose()
    }
    catch [Net.WebException] {
        if ($_.Exception.Response.StatusCode -ne [Net.FtpStatusCode]::ActionNotTakenFileUnavailable) {
            throw
        }
    }
}

$completedOperations = 0
try {
    foreach ($entry in $uploads) {
        Invoke-WithRetry { Upload-File $entry.Source $entry.Destination } "Upload $($entry.Destination)"
        $completedOperations++
    }

    foreach ($remotePath in $deletions) {
        Invoke-WithRetry { Remove-RemoteFile $remotePath } "Delete $remotePath"
        $completedOperations++
    }
}
catch {
    if ($completedOperations -gt 0) {
        Write-Warning "Altervista deployment failed after $completedOperations completed remote operations. Review the plan and remote state before retrying."
    }

    throw
}

Write-Output 'Targeted Portfolio.Web deployment to Altervista completed.'
