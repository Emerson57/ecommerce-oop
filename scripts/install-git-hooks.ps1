<#
Install git hooks locally by setting core.hooksPath to .githooks
Run: pwsh ./scripts/install-git-hooks.ps1
#>
param()

$root = Resolve-Path -Path "."
Write-Host "Setting git hooks path to .githooks in repository: $root"

git config core.hooksPath ".githooks"
if ($LASTEXITCODE -ne 0) { Write-Error "Failed to set git config core.hooksPath"; exit 1 }

if (-Not (Test-Path -Path ".githooks/pre-commit")) {
    Write-Host "Hook script not found in .githooks/pre-commit" -ForegroundColor Yellow
} else {
    # Ensure hook file is executable on UNIX systems via git's smudge/checkout; set file mode
    try {
        icacls .githooks\pre-commit /grant "Users:(RX)" | Out-Null
    } catch {
        # best-effort
    }
    Write-Host "Git hooks installed. Pre-commit will run secret scanner." -ForegroundColor Green
}

# Attempt to run pre-commit install if framework is present
if (Get-Command pre-commit -ErrorAction SilentlyContinue) {
    try {
        pre-commit install
        Write-Host "pre-commit framework installed hooks." -ForegroundColor Green
    } catch {
        Write-Warning "pre-commit present but installation failed: $_"
    }
} else {
    Write-Host "pre-commit framework not found. You can run 'pre-commit install' after installing pre-commit." -ForegroundColor Yellow
}
