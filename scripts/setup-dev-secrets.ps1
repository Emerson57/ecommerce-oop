<#
Setup dev user-secrets for PlataformaECommerce.Web

Usage (PowerShell):
  # from repo root
  .\scripts\setup-dev-secrets.ps1

Options:
  -ProjectDir <path>   Relative path to Web project (default: PlataformaECommerce.Web)
  -ConnectionString <string>  Optional DB connection string (if not provided a safe default for development is used)
  -JwtKeyLengthBytes <int>     Number of random bytes for JWT key (default 48)
  -Force                      Force re-init of user-secrets even if UserSecretsId exists

This script will:
  - cd into the web project folder
  - run `dotnet user-secrets init` if needed
  - generate a strong random SigningKey (Base64)
  - set user-secrets for:
      Secrets:Database:PrimaryConnectionString
      Secrets:Security:JwtSigningKey
  - print the values and guidance (does not print the secret in CI logs unless run locally)
#>
[CmdletBinding()]
param(
    [string]$ProjectDir = "PlataformaECommerce.Web",
    [string]$ConnectionString = "Server=.\SQLEXPRESS;Database=PlataformaECommerceDb;Trusted_Connection=True;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=True;",
    [int]$JwtKeyLengthBytes = 48,
    [switch]$Force
)

function Write-Info($msg) { Write-Host "[INFO] $msg" -ForegroundColor Cyan }
function Write-Warn($msg) { Write-Host "[WARN] $msg" -ForegroundColor Yellow }
function Write-ErrorExit($msg) { Write-Host "[ERROR] $msg" -ForegroundColor Red; exit 1 }

# Resolve paths
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Resolve-Path -Path "$scriptDir\.." | Select-Object -ExpandProperty Path
$projectPath = Join-Path $repoRoot $ProjectDir

if (-not (Test-Path $projectPath)) {
    Write-ErrorExit "Project directory not found: $projectPath"
}

Write-Info "Using project directory: $projectPath"
Push-Location $projectPath
try {
    # Ensure dotnet available
    if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
        Write-ErrorExit "dotnet CLI not found in PATH. Install .NET SDK (>= 6) and retry."
    }

    # Check for csproj
    $csproj = Get-ChildItem -Path $projectPath -Filter *.csproj -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1
    if (-not $csproj) { Write-Warn "No csproj found in $projectPath. Continuing but ensure you're pointing to the correct project." }
    else { Write-Info "Found project file: $($csproj.Name)" }

    # Initialize user-secrets (idempotent). If already initialized and not forcing, skip init message.
    $needsInit = $true
    if ($csproj) {
        $csprojContent = Get-Content -Path $csproj.FullName -Raw
        if ($csprojContent -match '<UserSecretsId>[^<]+</UserSecretsId>') {
            $needsInit = $false
            if ($Force) { $needsInit = $true }
        }
    }

    if ($needsInit) {
        Write-Info "Initializing user-secrets for project..."
        & dotnet user-secrets init
        if ($LASTEXITCODE -ne 0) { Write-ErrorExit "dotnet user-secrets init failed." }
    }
    else {
        Write-Info "UserSecretsId already present in csproj. Skipping init. Use -Force to re-init."
    }

    # Generate JWT signing key (Base64 of random bytes)
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    $bytes = New-Object byte[] $JwtKeyLengthBytes
    $rng.GetBytes($bytes)
    $jwtKey = [Convert]::ToBase64String($bytes)

    # Set user-secrets
    Write-Info "Setting user-secrets (local only) ..."
    & dotnet user-secrets set "Secrets:Database:PrimaryConnectionString" "$ConnectionString"
    if ($LASTEXITCODE -ne 0) { Write-ErrorExit "Failed to set Secrets:Database:PrimaryConnectionString" }

    & dotnet user-secrets set "Secrets:Security:JwtSigningKey" "$jwtKey"
    if ($LASTEXITCODE -ne 0) { Write-ErrorExit "Failed to set Secrets:Security:JwtSigningKey" }

    Write-Host "`nSetup complete." -ForegroundColor Green
    Write-Info "Database connection set to: $ConnectionString"
    Write-Info "JWT signing key generated and stored in user-secrets. (Length bytes: $JwtKeyLengthBytes)"
    Write-Host "Important: user-secrets are stored locally per user profile and are not versioned. Do NOT commit secrets." -ForegroundColor Yellow
    Write-Host "To view stored secrets (local): dotnet user-secrets list" -ForegroundColor DarkCyan
}
finally {
    Pop-Location
}
