param(
  [ValidateSet("Web", "WinUI3")]
  [string]$Stack = "Web",
  [Parameter(Mandatory=$false)]
  [string]$AppName,
  [Parameter(Mandatory=$false)]
  [string]$TargetDir
)

$ErrorActionPreference = "Stop"

if (-not $TargetDir) {
  if ($AppName) {
    $TargetDir = Join-Path $PWD.Path "..\$AppName"
  } else {
    $TargetDir = Join-Path $PWD.Path "..\my-app"
  }
}
New-Item -ItemType Directory -Force -Path $TargetDir | Out-Null

$AppName = if ($AppName) { $AppName } else { Split-Path $TargetDir -Leaf }

if ($Stack -eq "Web") {
  Push-Location $PWD.Path
  try {
    npx --yes create-next-app@latest --ts --tailwind --app --src-dir --eslint --import-alias "@/*" $TargetDir
    npx --yes shadcn@latest init --cwd $TargetDir
  } finally {
    Pop-Location
  }
  Write-Host "Web scaffold created in $TargetDir"
} else {
  $TemplateRoot = Split-Path $PSScriptRoot -Parent
  $CopyScript = Join-Path $TemplateRoot "scripts\copy-winui3-presets.ps1"
  if (Test-Path -LiteralPath $CopyScript) {
    & $CopyScript -Target $TargetDir
  }
  dotnet new winui3 --help | Out-Null
  if ($LASTEXITCODE -ne 0) {
    Write-Warning "WinUI templates not found. Presets copied. Run guides/03-winui3.md and create the solution manually in Visual Studio Template Studio for WinUI."
    exit 0
  }
  Write-Host "Creating WinUI3 solution in $TargetDir..."
  Write-Host "Follow guides/03-winui3.md for exact steps if dotnet new winui3 is unavailable."
}
