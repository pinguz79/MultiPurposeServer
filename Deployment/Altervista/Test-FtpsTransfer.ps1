[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$server = $env:ALTERVISTA_FTP_SERVER
$username = $env:ALTERVISTA_FTP_USERNAME
$password = $env:ALTERVISTA_FTP_PASSWORD
$expectedCertificateSha256 = ([string] $env:ALTERVISTA_FTP_CERTIFICATE_SHA256).Replace(' ', '').ToUpperInvariant()
$remotePath = '.codex-altervista-ftps-transfer-test.txt'
$content = [Text.Encoding]::UTF8.GetBytes("Portfolio.Web FTPS transfer test`n")

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

$uploadAttempted = $false
try {
    $uploadRequest = New-FtpsRequest ([Net.WebRequestMethods+Ftp]::UploadFile)
    $uploadAttempted = $true
    $uploadRequest.ContentLength = $content.Length
    $uploadStream = $uploadRequest.GetRequestStream()
    try {
        $uploadStream.Write($content, 0, $content.Length)
    }
    finally {
        $uploadStream.Dispose()
    }

    $uploadResponse = $uploadRequest.GetResponse()
    $uploadResponse.Dispose()
    $downloadRequest = New-FtpsRequest ([Net.WebRequestMethods+Ftp]::DownloadFile)
    $downloadResponse = $downloadRequest.GetResponse()
    try {
        $memory = [IO.MemoryStream]::new()
        try {
            $downloadResponse.GetResponseStream().CopyTo($memory)
            $downloaded = $memory.ToArray()
        }
        finally {
            $memory.Dispose()
        }
    }
    finally {
        try {
            $downloadResponse.Dispose()
        }
        catch [Net.WebException] {
            if ($_.Exception.Response.StatusCode -ne [Net.FtpStatusCode]::LocalError) {
                throw
            }

            Write-Warning 'Altervista returned FTP 451 while closing the completed download; downloaded bytes will still be verified.'
        }
    }

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
}
