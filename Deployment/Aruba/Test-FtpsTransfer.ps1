[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$server = $env:ARUBA_FTP_SERVER
$username = $env:ARUBA_FTP_USERNAME
$password = $env:ARUBA_FTP_PASSWORD
$remotePath = 'codex-aruba-ftps-transfer-test.txt'
$content = [Text.Encoding]::UTF8.GetBytes("MPS Aruba FTPS transfer test`n")
$temporaryDirectory = Join-Path ([IO.Path]::GetTempPath()) ("mps-aruba-ftps-" + [Guid]::NewGuid().ToString('N'))
$uploadPath = Join-Path $temporaryDirectory 'upload.txt'
$downloadPath = Join-Path $temporaryDirectory 'download.txt'

if ([string]::IsNullOrWhiteSpace($server) -or
    [string]::IsNullOrWhiteSpace($username) -or
    [string]::IsNullOrWhiteSpace($password)) {
    throw 'ARUBA_FTP_SERVER, ARUBA_FTP_USERNAME and ARUBA_FTP_PASSWORD are required.'
}

$credential = [Net.NetworkCredential]::new($username, $password)
$curlCommand = @(Get-Command curl -CommandType Application -ErrorAction SilentlyContinue) | Select-Object -First 1
if ($null -eq $curlCommand) {
    throw 'curl is required for the Aruba FTPS data-channel test.'
}

function New-FtpsRequest([string] $Method) {
    $request = [Net.FtpWebRequest]::Create("ftp://$server/$remotePath")
    $request.Method = $Method
    $request.Credentials = $credential
    $request.EnableSsl = $true
    $request.UsePassive = $true
    $request.KeepAlive = $false
    $request.Timeout = 30000
    $request.ReadWriteTimeout = 30000
    return $request
}

function Invoke-CurlFtps([string[]] $Arguments) {
    $env:CURL_USERPWD = "$username`:$password"
    try {
        & $curlCommand.Path --fail --silent --show-error --ssl-reqd --ftp-pasv --user $env:CURL_USERPWD @Arguments
        if ($LASTEXITCODE -ne 0) {
            throw "curl FTPS operation failed with exit code $LASTEXITCODE."
        }
    }
    finally {
        Remove-Item Env:CURL_USERPWD -ErrorAction SilentlyContinue
    }
}

$uploadAttempted = $false
try {
    [IO.Directory]::CreateDirectory($temporaryDirectory) | Out-Null
    [IO.File]::WriteAllBytes($uploadPath, $content)
    $uploadAttempted = $true
    Invoke-CurlFtps @('--upload-file', $uploadPath, "ftp://$server/$remotePath")
    Invoke-CurlFtps @('--output', $downloadPath, "ftp://$server/$remotePath")
    $downloaded = [IO.File]::ReadAllBytes($downloadPath)

    $expectedHash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($content))
    $actualHash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($downloaded))
    if ($expectedHash -ne $actualHash) {
        throw 'The downloaded Aruba sentinel does not match the uploaded content.'
    }

    Write-Output "FTPS upload, download and content verification succeeded for $remotePath."
}
finally {
    if ($uploadAttempted) {
        try {
            $deleteRequest = New-FtpsRequest ([Net.WebRequestMethods+Ftp]::DeleteFile)
            $deleteResponse = $deleteRequest.GetResponse()
            $deleteResponse.Dispose()
            Write-Output "Temporary sentinel $remotePath was removed."
        }
        catch [Net.WebException] {
            if ($_.Exception.Response.StatusCode -ne [Net.FtpStatusCode]::ActionNotTakenFileUnavailable) {
                Write-Warning "Unable to remove the temporary sentinel $remotePath automatically: $($_.Exception.Message)"
            }
        }
    }

    if (Test-Path -LiteralPath $temporaryDirectory) {
        Remove-Item -LiteralPath $temporaryDirectory -Recurse -Force
    }
}
