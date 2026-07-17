$extensions = @(
    ".php",
    ".html",
    ".htm",
    ".css",
    ".js",
    ".json",
    ".xml",
    ".svg",
    ".txt",
    ".md",
    ".ini",
    ".htaccess"
)

$utf8NoBom = New-Object System.Text.UTF8Encoding($false)
$converted = 0
$errors = 0

Get-ChildItem -Path $PSScriptRoot -File -Recurse | Where-Object {
    $extensions -contains $_.Extension.ToLowerInvariant() -or $_.Name -eq ".htaccess"
} | ForEach-Object {
    try {
        $content = [System.IO.File]::ReadAllText($_.FullName)
        [System.IO.File]::WriteAllText($_.FullName, $content, $utf8NoBom)

        Write-Host "Convertito: $($_.FullName)"
        $converted++
    }
    catch {
        Write-Warning "Errore su $($_.FullName): $($_.Exception.Message)"
        $errors++
    }
}

Write-Host ""
Write-Host "Conversione completata."
Write-Host "File convertiti: $converted"
Write-Host "Errori: $errors"