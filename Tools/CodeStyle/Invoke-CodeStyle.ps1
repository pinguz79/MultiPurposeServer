param(
    [switch] $Fix
)

$ErrorActionPreference = 'Stop'

$repositoryRoot = Resolve-Path (Join-Path $PSScriptRoot '..\..')
$solutionPath = Join-Path $repositoryRoot 'MultiPurposeServer.slnx'
$commonArguments = @(
    $solutionPath
    '--no-restore'
    '--verbosity'
    'minimal'
)

function Invoke-DotNetFormat {
    param(
        [Parameter(Mandatory)]
        [string] $Category,

        [string[]] $AdditionalArguments = @()
    )

    $arguments = @('format') + $commonArguments + $Category + $AdditionalArguments

    if (-not $Fix) {
        $arguments += '--verify-no-changes'
    }

    & dotnet $arguments

    if ($LASTEXITCODE -ne 0) {
        throw "La verifica '$Category' è fallita con codice $LASTEXITCODE."
    }
}

Push-Location $repositoryRoot

try {
    & (Join-Path $PSScriptRoot 'Test-CSharpStructure.ps1') -RepositoryRoot $repositoryRoot.Path

    Invoke-DotNetFormat -Category 'whitespace'
    Invoke-DotNetFormat -Category 'style' -AdditionalArguments @(
        '--diagnostics'
        'IDE0005'
        'IDE0011'
        'IDE0160'
    )
}
finally {
    Pop-Location
}

$operation = if ($Fix) { 'Correzione' } else { 'Verifica' }
Write-Host "$operation delle regole deterministiche completata."
