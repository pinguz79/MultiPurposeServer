[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$server = $env:ALTERVISTA_FTP_SERVER
$username = $env:ALTERVISTA_FTP_USERNAME
$password = $env:ALTERVISTA_FTP_PASSWORD
$expectedCertificateSha256 = ([string] $env:ALTERVISTA_FTP_CERTIFICATE_SHA256).Replace(' ', '').ToUpperInvariant()

if ([string]::IsNullOrWhiteSpace($server) -or
    [string]::IsNullOrWhiteSpace($username) -or
    [string]::IsNullOrWhiteSpace($password) -or
    [string]::IsNullOrWhiteSpace($expectedCertificateSha256)) {
    throw 'ALTERVISTA_FTP_SERVER, ALTERVISTA_FTP_USERNAME, ALTERVISTA_FTP_PASSWORD and ALTERVISTA_FTP_CERTIFICATE_SHA256 are required.'
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

try {
    $request = [Net.FtpWebRequest]::Create("ftp://$server/")
    $request.Method = [Net.WebRequestMethods+Ftp]::PrintWorkingDirectory
    $request.Credentials = [Net.NetworkCredential]::new($username, $password)
    $request.EnableSsl = $true
    $request.UsePassive = $true
    $request.KeepAlive = $false
    $request.Timeout = 30000
    $request.ReadWriteTimeout = 30000

    $response = $request.GetResponse()
    try {
        $workingDirectory = $response.StatusDescription.Trim()
    }
    finally {
        $response.Dispose()
    }
}
finally {
    [Net.ServicePointManager]::ServerCertificateValidationCallback = $previousCertificateValidationCallback
}

Write-Output "FTPS authentication to $server succeeded. Server response: $workingDirectory"
Write-Output 'No remote file was created, modified or deleted.'
