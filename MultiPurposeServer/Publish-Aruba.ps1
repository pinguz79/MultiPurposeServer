[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$projectDirectory = (Resolve-Path -LiteralPath $PSScriptRoot).Path
$projectPath = Join-Path $projectDirectory 'MultiPurposeServer.csproj'
$publishPath = Join-Path $projectDirectory 'bin\Publish\net10.0'
$expectedPrefix = $projectDirectory + [IO.Path]::DirectorySeparatorChar

if (-not $publishPath.StartsWith($expectedPrefix, [StringComparison]::OrdinalIgnoreCase)) {
    throw "Publish path is outside the project directory: $publishPath"
}

if (Test-Path -LiteralPath $publishPath) {
    $cleaned = $false

    foreach ($attempt in 1..5) {
        try {
            Get-ChildItem -LiteralPath $publishPath -Recurse -File | Remove-Item -Force -ErrorAction Stop
            Get-ChildItem -LiteralPath $publishPath -Recurse -Directory |
                Sort-Object FullName -Descending |
                Remove-Item -Force -ErrorAction SilentlyContinue

            if (-not (Get-ChildItem -LiteralPath $publishPath -Recurse -File)) {
                $cleaned = $true
                break
            }
        }
        catch {
            if ($attempt -ge 5) {
                break
            }

            Start-Sleep -Milliseconds (250 * $attempt)
        }
    }

    if (-not $cleaned) {
        throw "Unable to clean the Aruba publish artifacts because a file is in use: $publishPath"
    }
}

& dotnet publish $projectPath `
    --no-restore `
    '-p:PublishProfile=Aruba' `
    "-p:PublishDir=$publishPath"

if ($LASTEXITCODE -ne 0) {
    throw "Aruba publish failed with exit code $LASTEXITCODE."
}

Write-Output "Aruba publish completed: $publishPath"
