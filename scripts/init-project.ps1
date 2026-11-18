<#
.SYNOPSIS
  Initializes a new project repository created from this template.
.DESCRIPTION
  Sets up Git, activates pre-commit hooks, and untracks agent/template files
  from the Git index while keeping them on disk for Antigravity.
#>
param(
  [switch]$SkipGitInit = $false
)

$ErrorActionPreference = "Stop"

$ProjectRoot = Split-Path $PSScriptRoot -Parent
Push-Location $ProjectRoot

try {
  Write-Host "Initializing new project from template..." -ForegroundColor Cyan

  $IsGitRepo = Test-Path -LiteralPath (Join-Path $ProjectRoot ".git")

  if (-not $IsGitRepo -and -not $SkipGitInit) {
    Write-Host "Initializing git repository (main branch)..."
    git init -b main
    $IsGitRepo = $true
  }

  if ($IsGitRepo) {
    Write-Host "Configuring Git hooks path to .githooks..."
    git config core.hooksPath .githooks

    # Check if agent files are currently tracked in Git index (e.g. from GitHub 'Use this template')
    $targets = @("AGENTS.md", ".agents", ".gemini", ".claude", ".cursor", "handoff.md", "guides", "docs/presets")
    $tracked = @()

    foreach ($t in $targets) {
      $files = git ls-files $t 2>$null
      if ($files) {
        $tracked += $t
      }
    }

    if ($tracked.Count -gt 0) {
      Write-Host "Untracking agent/template files from Git index ($($tracked -join ', '))..."
      git rm -r --cached --ignore-unmatch AGENTS.md .agents .gemini .claude .cursor handoff.md guides docs/presets 2>$null | Out-Null
      Write-Host "Files successfully untracked from Git (persisted locally on disk)." -ForegroundColor Green
    } else {
      Write-Host "No tracked agent files detected in index. .gitignore will handle future exclusions."
    }
  }

  Write-Host ""
  Write-Host "Initialization complete!" -ForegroundColor Green
  Write-Host "  * Antigravity skills and AGENTS.md remain available locally on disk."
  Write-Host "  * Git will ignore all agent and template scaffolding files."
  Write-Host "  * Pre-commit hook in .githooks/ will block accidental staging."
  Write-Host ""
  Write-Host "Next step: Run /discovery in Antigravity or see guides/01-setup-and-discovery.md"
}
finally {
  Pop-Location
}
