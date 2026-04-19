<#
Bootstrap script to set up developer environment for this repository.
- Sets git core.hooksPath to .githooks
- Installs pre-commit framework if available
- Ensures .githooks/pre-commit is executable on non-Windows systems
- Provides guidance for developers
#>
param()

Write-Host "Repository bootstrap: configuring git hooks and pre-commit"

try {
    git config core.hooksPath ".githooks"
    if ($LASTEXITCODE -ne 0) { throw "git config failed" }
    Write-Host "Set git core.hooksPath to .githooks"
} catch {
    Write-Warning "Could not set git core.hooksPath automatically. Please run: git config core.hooksPath .githooks"
}

# Make pre-commit hook executable on UNIX systems (best-effort)
if (Test-Path ".githooks/pre-commit") {
    try {
        if (Test-Path env:WINDIR) {
            # Windows: try to set generic read-execute
            icacls .githooks\pre-commit /grant "Users:(RX)" | Out-Null
        } else {
            # Unix-like: set +x
            chmod +x .githooks/pre-commit
        }
        Write-Host "Pre-commit hook file permissions adjusted"
    } catch {
        Write-Warning "Could not adjust pre-commit file permissions: $_"
    }
}

# Install pre-commit framework (Python-based) if python and pip available
if (Get-Command python -ErrorAction SilentlyContinue) {
    try {
        python -m pip install --user pre-commit | Out-Null
        Write-Host "Installed pre-commit (user). Run 'pre-commit install' to finish setup." 
    } catch {
        Write-Warning "Could not install pre-commit via pip: $_"
    }
} elseif (Get-Command pip -ErrorAction SilentlyContinue) {
    try {
        pip install --user pre-commit | Out-Null
        Write-Host "Installed pre-commit (user). Run 'pre-commit install' to finish setup." 
    } catch {
        Write-Warning "Could not install pre-commit via pip: $_"
    }
} else {
    Write-Host "Python/pip not found. Skipping pre-commit installation. You can still use the .githooks pre-commit script." -ForegroundColor Yellow
}

Write-Host "Bootstrap complete. To finish pre-commit framework setup, run: pre-commit install" -ForegroundColor Green
