$root = $PSScriptRoot
$outputFile = Join-Path $root "portfolio-file-list.txt"

Get-ChildItem -Path $root -Recurse -File |
    Where-Object { $_.FullName -ne $outputFile } |
    Sort-Object FullName |
    ForEach-Object {
        $_.FullName.Substring($root.Length).TrimStart('\')
    } |
    Set-Content -Path $outputFile -Encoding UTF8

Write-Host "Esportazione completata: $outputFile"