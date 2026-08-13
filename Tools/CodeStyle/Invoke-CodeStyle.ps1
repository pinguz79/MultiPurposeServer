param(
    [switch] $Fix
)

$ErrorActionPreference = 'Stop'

$repositoryRoot = Resolve-Path (Join-Path $PSScriptRoot '..\..')
$projectPaths = Get-ChildItem -Path $repositoryRoot -Recurse -Filter '*.csproj' |
    Where-Object { $_.FullName -notmatch '[\\/]bin[\\/]|[\\/]obj[\\/]|SampleApp\.Mobile\.csproj$|Documentation\.csproj$' } |
    Select-Object -ExpandProperty FullName

function Invoke-DotNetFormat {
    param(
        [Parameter(Mandatory)]
        [string] $Category,

        [string[]] $AdditionalArguments = @()
    )

    foreach ($projectPath in $projectPaths) {
        $arguments = @('format', $projectPath, '--no-restore', '--verbosity', 'minimal', $Category) + $AdditionalArguments

        if (-not $Fix) {
            $arguments += '--verify-no-changes'
        }

        & dotnet $arguments

        if ($LASTEXITCODE -ne 0) {
            throw "La verifica '$Category' del progetto '$projectPath' è fallita con codice $LASTEXITCODE."
        }
    }
}

Push-Location $repositoryRoot

try {
    & (Join-Path $PSScriptRoot 'Test-CSharpStructure.ps1') -RepositoryRoot $repositoryRoot.Path

    Invoke-DotNetFormat -Category 'whitespace'
    Invoke-DotNetFormat -Category 'style' -AdditionalArguments @(
        '--diagnostics'
        'IDE0001'
        'IDE0005'
        'IDE0011'
        'IDE0046'
        'IDE0305'
        'IDE0160'
    )
}
finally {
    Pop-Location
}

$operation = if ($Fix) { 'Correzione' } else { 'Verifica' }
Write-Host "$operation delle regole deterministiche completata."
