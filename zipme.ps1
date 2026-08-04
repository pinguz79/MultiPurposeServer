Add-Type -AssemblyName System.IO.Compression.FileSystem

$Root = (Get-Location).Path
$ZipFile = Join-Path $Root "MultiPurposeServer-CodeReview.zip"

if (Test-Path $ZipFile) {
    Remove-Item $ZipFile -Force
}

$zip = [System.IO.Compression.ZipFile]::Open($ZipFile, "Create")

try {

    Get-ChildItem $Root -Recurse -File |  Where-Object {

        $_.FullName -ne $ZipFile `
        -and $_.FullName -notmatch '\\bin\\' `
        -and $_.FullName -notmatch '\\obj\\' `
        -and $_.FullName -notmatch '\\\.vs\\' `
        -and $_.FullName -notmatch '\\\.git\\' `
        -and $_.FullName -notmatch '\\MultiPurposeServer\\Portfolio\\' `
        -and $_.FullName -notmatch '\\[Ll]ogs?\\'
        -and $_.FullName -notmatch '\\TestResults\\'
        -and $_.Extension -ne ".db"
        -and $_.Extension -ne ".zip"
        -and $_.Extension -notin @(
            ".user",
            ".suo",
            ".userprefs",
            ".cache"
        )

    } | ForEach-Object {

        $relativePath = $_.FullName.Substring($Root.Length + 1)

        [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile(
            $zip,
            $_.FullName,
            $relativePath,
            [System.IO.Compression.CompressionLevel]::Optimal
        ) | Out-Null
    }

}
finally {
    $zip.Dispose()
}

Write-Host ""
Write-Host "ZIP creato:"
Write-Host $ZipFile