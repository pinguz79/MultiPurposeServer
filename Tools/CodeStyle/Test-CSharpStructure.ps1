param(
    [Parameter(Mandatory)]
    [string] $RepositoryRoot
)

$ErrorActionPreference = 'Stop'
$violations = [Collections.Generic.List[string]]::new()
$typePattern = '(?m)^\s*(?:public|internal|private|protected)?\s*(?:(?:sealed|abstract|static|partial|readonly|ref)\s+)*(?:class|record(?:\s+struct)?|struct|interface|enum)\s+([A-Za-z_]\w*)'
$namespacePattern = '(?m)^namespace\s+([^\s{;]+)'

Get-ChildItem $RepositoryRoot -Recurse -Filter *.csproj | Where-Object {
    $_.FullName -notmatch '\\(?:bin|obj)\\'
} | ForEach-Object {
    $project = $_
    $projectRoot = $project.Directory.FullName.TrimEnd('\')
    $projectName = $project.BaseName

    Get-ChildItem $projectRoot -Recurse -Filter *.cs | Where-Object {
        $_.FullName -notmatch '\\(?:bin|obj|Migrations|Resources)\\' -and
        $_.Name -notlike '*.Designer.cs' -and
        $_.Name -notlike '*.g.cs' -and
        $_.Name -notlike '*.g.i.cs'
    } | ForEach-Object {
        $file = $_
        $relativePath = $file.FullName.Substring($RepositoryRoot.Length).TrimStart('\')
        $content = Get-Content -Raw -LiteralPath $file.FullName
        $types = @([regex]::Matches($content, $typePattern) | ForEach-Object { $_.Groups[1].Value })
        $isTopLevelProgram = $file.Name -eq 'Program.cs' -and $types.Count -eq 0

        if (-not $isTopLevelProgram -and $types.Count -ne 1) {
            $violations.Add("$relativePath`: atteso un solo tipo, trovati $($types.Count).")
        }
        else {
            $expectedTypeName = if ($file.BaseName.EndsWith('.xaml')) {
                [IO.Path]::GetFileNameWithoutExtension($file.BaseName)
            }
            else {
                $file.BaseName
            }

            if ($types[0] -ne $expectedTypeName) {
                $violations.Add("$relativePath`: il tipo '$($types[0])' non coincide con il nome del file '$expectedTypeName'.")
            }
        }

        $namespaceMatch = [regex]::Match($content, $namespacePattern)
        if ($isTopLevelProgram) {
            return
        }

        if (-not $namespaceMatch.Success) {
            $violations.Add("$relativePath`: namespace assente.")
            return
        }

        $relativeDirectory = $file.Directory.FullName.Substring($projectRoot.Length).TrimStart('\')
        $namespaceSuffix = if ([string]::IsNullOrWhiteSpace($relativeDirectory)) {
            ''
        }
        else {
            ".$(($relativeDirectory -replace '\\', '.'))"
        }
        $expectedNamespace = "$projectName$namespaceSuffix"
        $actualNamespace = $namespaceMatch.Groups[1].Value

        if ($actualNamespace -ne $expectedNamespace) {
            $violations.Add("$relativePath`: namespace '$actualNamespace', atteso '$expectedNamespace'.")
        }
    }
}

if ($violations.Count -gt 0) {
    $violations | ForEach-Object { Write-Error $_ }
    throw "Verifica strutturale C# fallita: $($violations.Count) violazioni."
}

Write-Host 'Verifica strutturale C# completata.'
