[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$server = $env:ALTERVISTA_FTP_SERVER
$username = $env:ALTERVISTA_FTP_USERNAME
$password = $env:ALTERVISTA_FTP_PASSWORD
$expectedCertificateSha256 = ([string] $env:ALTERVISTA_FTP_CERTIFICATE_SHA256).Replace(' ', '').ToUpperInvariant()
$remotePath = '.codex-altervista-ftps-transfer-test.txt'
$content = [Text.Encoding]::UTF8.GetBytes("Portfolio.Web FTPS transfer test`n")
$temporaryDirectory = Join-Path ([IO.Path]::GetTempPath()) ("portfolio-web-ftps-" + [Guid]::NewGuid().ToString('N'))
$uploadPath = Join-Path $temporaryDirectory 'upload.txt'
$downloadPath = Join-Path $temporaryDirectory 'download.txt'

if ([string]::IsNullOrWhiteSpace($server) -or
    [string]::IsNullOrWhiteSpace($username) -or
    [string]::IsNullOrWhiteSpace($password) -or
    [string]::IsNullOrWhiteSpace($expectedCertificateSha256)) {
    throw 'ALTERVISTA_FTP_SERVER, ALTERVISTA_FTP_USERNAME, ALTERVISTA_FTP_PASSWORD and ALTERVISTA_FTP_CERTIFICATE_SHA256 are required.'
}

$curlCommand = Get-Command curl -CommandType Application -ErrorAction SilentlyContinue
if ($null -eq $curlCommand) {
    throw 'curl is required for the Altervista FTPS data-channel test.'
}

$script:altervistaExpectedCertificateSha256 = $expectedCertificateSha256
$previousCertificateValidationCallback = [Net.ServicePointManager]::ServerCertificateValidationCallback
[Net.ServicePointManager]::ServerCertificateValidationCallback = {
    param($sender, $certificate, $chain, $sslPolicyErrors)

    if ($sslPolicyErrors -eq [Net.Security.SslPolicyErrors]::None) {
        return $true
    }

    if ($sslPolicyErrors -ne [Net.Security.SslPolicyErrors]::RemoteCertificateNameMismatch -or $null -eq $certificate) {
        return $false
    }

    $remoteCertificate = [Security.Cryptography.X509Certificates.X509Certificate2]::new($certificate)
    $actualSha256 = $remoteCertificate.GetCertHashString([Security.Cryptography.HashAlgorithmName]::SHA256)
    return $actualSha256.Equals($script:altervistaExpectedCertificateSha256, [StringComparison]::OrdinalIgnoreCase)
}

$credential = [Net.NetworkCredential]::new($username, $password)

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
        & $curlCommand.Source --fail --silent --show-error --ssl-reqd --insecure --ftp-pasv --user $env:CURL_USERPWD @Arguments
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
    # Validate the expected Altervista certificate immediately before curl uses the same host.
    $preflightRequest = New-FtpsRequest ([Net.WebRequestMethods+Ftp]::PrintWorkingDirectory)
    $preflightResponse = $preflightRequest.GetResponse()
    $preflightResponse.Dispose()

    [IO.Directory]::CreateDirectory($temporaryDirectory) | Out-Null
    [IO.File]::WriteAllBytes($uploadPath, $content)
    $uploadAttempted = $true

    Invoke-CurlFtps @('--upload-file', $uploadPath, "ftp://$server/$remotePath")
    Invoke-CurlFtps @('--output', $downloadPath, "ftp://$server/$remotePath")

    $downloaded = [IO.File]::ReadAllBytes($downloadPath)
    $sha256 = [Security.Cryptography.SHA256]::Create()
    try {
        $expectedHash = [Convert]::ToBase64String($sha256.ComputeHash($content))
        $actualHash = [Convert]::ToBase64String($sha256.ComputeHash($downloaded))
    }
    finally {
        $sha256.Dispose()
    }

    if ($expectedHash -ne $actualHash) {
        throw 'The downloaded sentinel does not match the uploaded content.'
    }

    Write-Output "FTPS upload and download succeeded for the temporary sentinel $remotePath."
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

    [Net.ServicePointManager]::ServerCertificateValidationCallback = $previousCertificateValidationCallback

    if (Test-Path -LiteralPath $temporaryDirectory) {
        Remove-Item -LiteralPath $temporaryDirectory -Recurse -Force
    }
}
