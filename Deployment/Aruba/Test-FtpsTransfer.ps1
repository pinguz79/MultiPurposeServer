[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$server = $env:ARUBA_FTP_SERVER
$username = $env:ARUBA_FTP_USERNAME
$password = $env:ARUBA_FTP_PASSWORD
$remotePath = 'codex-aruba-ftps-transfer-test.txt'
$content = [Text.Encoding]::UTF8.GetBytes("MPS Aruba FTPS transfer test`n")

if ([string]::IsNullOrWhiteSpace($server) -or
    [string]::IsNullOrWhiteSpace($username) -or
    [string]::IsNullOrWhiteSpace($password)) {
    throw 'ARUBA_FTP_SERVER, ARUBA_FTP_USERNAME and ARUBA_FTP_PASSWORD are required.'
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
    $uploadRequest.ContentLength = $content.Length
    $uploadAttempted = $true
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
        $downloadResponse.Dispose()
    }

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
}
