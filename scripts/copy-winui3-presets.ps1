param(
  [string]$Target = $PWD.Path
)

$Source = Join-Path $PSScriptRoot "..\docs\presets"
$Files = @("WINUI3_STARTER_GUIDE.md", "WINUI3_UPDATE_WORKFLOW.md")

if (-not (Test-Path -LiteralPath $Source)) {
  Write-Error "Presets folder not found: $Source"
  exit 1
}

$Dest = Join-Path $Target "docs"
New-Item -ItemType Directory -Force -Path $Dest | Out-Null

foreach ($f in $Files) {
  $srcFile = Join-Path $Source $f
  if (Test-Path -LiteralPath $srcFile) {
    Copy-Item -Force -LiteralPath $srcFile -Destination (Join-Path $Dest $f)
    Write-Host "Copied $f -> $Dest"
  } else {
    Write-Host "Missing $f in $Source"
  }
}
