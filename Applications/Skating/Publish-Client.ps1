[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^\d+\.\d+\.\d+(?:-[A-Za-z0-9]+(?:[.-][A-Za-z0-9]+)*)?$')]
    [string]$Version,
    [ValidateSet('None', 'Discipline', 'Category')]
    [string]$Aggregation = 'None'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$repository = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$outputRoot = Join-Path $repository 'artifacts/Skating'
$buildId = '{0}-{1}' -f (Get-Date -Format 'yyyyMMdd-HHmmss'), ([Guid]::NewGuid().ToString('N').Substring(0, 8))
$packageName = "Skating-System-$Version-win-x64"
$buildRoot = Join-Path $outputRoot "$packageName-$buildId"
$packageRoot = Join-Path $buildRoot $packageName
$appRoot = Join-Path $packageRoot 'App'
$project = Join-Path $PSScriptRoot 'Skating.Desktop/Skating.Desktop.csproj'

function Invoke-Dotnet {
    param([string[]]$Arguments)
    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) { throw "dotnet non riuscito (codice $LASTEXITCODE). Nessun pacchetto da distribuire." }
}

Push-Location $repository
try {
    # Destinazione nuova a ogni esecuzione: nessuna cancellazione di pacchetti precedenti.
    foreach ($testProject in @('Tests/Skating/Skating.Tests/Skating.Tests.csproj', 'Tests/Skating/Skating.Desktop.Tests/Skating.Desktop.Tests.csproj')) {
        Invoke-Dotnet -Arguments @('test', $testProject, '-c', 'Release')
    }
    New-Item -ItemType Directory -Path $appRoot | Out-Null
    Invoke-Dotnet -Arguments @('publish', $project, '-c', 'Release', '-r', 'win-x64', '--self-contained', 'true', '-o', $appRoot,
        "-p:Version=$Version", '-p:PublishSingleFile=false', '-p:PublishTrimmed=false', '-p:DebugType=None', '-p:DebugSymbols=false')

    $encoding = [Text.UTF8Encoding]::new($false)
    $mode = if ($Aggregation -eq 'None') { $null } else { $Aggregation }
    $configuration = @{ IntermediateAwardAggregation = $mode } | ConvertTo-Json
    [IO.File]::WriteAllText((Join-Path $appRoot 'skating-settings.json'), $configuration + "`r`n", $encoding)
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'Distribuzione/LEGGIMI.txt') -Destination $packageRoot
    foreach ($required in @('Skating.Desktop.exe', 'Skating.Desktop.dll', 'coreclr.dll', 'System.Windows.Forms.dll', 'e_sqlite3.dll', 'skating-settings.json')) {
        if (-not (Test-Path -LiteralPath (Join-Path $appRoot $required) -PathType Leaf)) { throw "Publish incompleto: manca $required." }
    }
    $forbidden = @(Get-ChildItem -LiteralPath $packageRoot -File -Recurse | Where-Object { $_.Extension -in @('.skating', '.db', '.sqlite', '.bak', '.pdb', '.user') })
    if ($forbidden.Count -gt 0) { throw 'Il pacchetto contiene dati o file non distribuibili.' }

    $revision = & git rev-parse HEAD
    if ($LASTEXITCODE -ne 0) { throw 'Impossibile identificare il commit del pacchetto.' }
    $changes = @(& git status --porcelain --untracked-files=all -- Applications/Skating Tests/Skating Directory.Build.props Directory.Build.targets Directory.Packages.props global.json NuGet.Config)
    if ($LASTEXITCODE -ne 0) { throw 'Impossibile verificare lo stato dei sorgenti.' }
    $metadata = [ordered]@{ Version = $Version; Commit = "$revision"; ModifiedSources = $changes.Count -gt 0; BuiltAt = [DateTimeOffset]::Now.ToString('o'); Runtime = 'win-x64'; SelfContained = $true; Aggregation = $mode }
    [IO.File]::WriteAllText((Join-Path $packageRoot 'versione.json'), ($metadata | ConvertTo-Json) + "`r`n", $encoding)
    $inventory = @(Get-ChildItem -LiteralPath $appRoot -File -Recurse | Sort-Object FullName | ForEach-Object {
        [ordered]@{ File = [IO.Path]::GetRelativePath($packageRoot, $_.FullName); Bytes = $_.Length; Sha256 = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash }
    })
    [IO.File]::WriteAllText((Join-Path $packageRoot 'contenuto-runtime.json'), (ConvertTo-Json -InputObject $inventory -Depth 3) + "`r`n", $encoding)
    $archive = Join-Path $buildRoot "$packageName.zip"
    Compress-Archive -LiteralPath $packageRoot -DestinationPath $archive -CompressionLevel Optimal
    $hash = (Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash
    [IO.File]::WriteAllText("$archive.sha256", "$hash  $packageName.zip`r`n", $encoding)
    Write-Host "Pacchetto: $archive"
    Write-Host ('Dimensione: {0:N2} MB' -f ((Get-Item -LiteralPath $archive).Length / 1MB))
    Write-Host 'Estrarre lo ZIP e provare App/Skating.Desktop.exe prima della consegna. Nessun invio automatico.'
    if ($changes.Count -gt 0) { Write-Warning 'Il pacchetto comprende modifiche non committate, indicate in versione.json.' }
}
finally {
    Pop-Location
}
