[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$server = $env:ARUBA_FTP_SERVER
$username = $env:ARUBA_FTP_USERNAME
$password = $env:ARUBA_FTP_PASSWORD

if ([string]::IsNullOrWhiteSpace($server) -or
    [string]::IsNullOrWhiteSpace($username) -or
    [string]::IsNullOrWhiteSpace($password)) {
    throw 'ARUBA_FTP_SERVER, ARUBA_FTP_USERNAME and ARUBA_FTP_PASSWORD are required.'
}

$request = [Net.FtpWebRequest]::Create("ftp://$server/")
$request.Method = [Net.WebRequestMethods+Ftp]::ListDirectory
$request.Credentials = [Net.NetworkCredential]::new($username, $password)
$request.EnableSsl = $true
$request.UsePassive = $true
$request.KeepAlive = $false
$request.Timeout = 30000
$request.ReadWriteTimeout = 30000

$response = $request.GetResponse()
try {
    $reader = [IO.StreamReader]::new($response.GetResponseStream())
    try {
        $entries = @($reader.ReadToEnd() -split "`r?`n" | Where-Object { $_ })
    }
    finally {
        $reader.Dispose()
    }
}
finally {
    $response.Dispose()
}

Write-Output "FTPS connection to $server succeeded. Root entries visible: $($entries.Count)."
Write-Output 'No remote file was created, modified or deleted.'
