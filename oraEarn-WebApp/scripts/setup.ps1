# First-time local setup. From repo root:  .\scripts\setup.ps1
$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $PSScriptRoot

function Write-Step($message) { Write-Host $message -ForegroundColor Cyan }
function Write-Ok($message) { Write-Host "[OK] $message" -ForegroundColor Green }
function Write-Warn($message) { Write-Host "[!] $message" -ForegroundColor Yellow }

Write-Host ""
Write-Host "OraEarn setup" -ForegroundColor Cyan
Write-Host "Root: $Root"
Write-Host ""

foreach ($cmd in @("dotnet", "node", "npm")) {
    if (-not (Get-Command $cmd -ErrorAction SilentlyContinue)) {
        throw "$cmd is not on PATH. Install it, then run this script again."
    }
}

# Start local PostgreSQL if the Windows service exists and is stopped.
$pgService = Get-Service -Name "postgresql*" -ErrorAction SilentlyContinue | Select-Object -First 1
if ($pgService) {
    if ($pgService.Status -ne "Running") {
        Write-Step "Starting PostgreSQL ($($pgService.Name))..."
        try {
            Start-Service $pgService.Name
            Write-Ok "PostgreSQL is running"
        } catch {
            Write-Warn "Could not start PostgreSQL automatically. Start it from Services, then re-run."
        }
    } else {
        Write-Ok "PostgreSQL is already running"
    }
} else {
    Write-Warn "No PostgreSQL Windows service found. Install PostgreSQL 16+ and keep it running on port 5432."
}

# Frontend
$Frontend = Join-Path $Root "frontend"
$EnvExample = Join-Path $Frontend ".env.example"
$EnvFile = Join-Path $Frontend ".env"
if (-not (Test-Path $EnvFile) -and (Test-Path $EnvExample)) {
    Copy-Item $EnvExample $EnvFile
    Write-Ok "Created frontend .env (API URLs — you normally leave this as-is)"
}

# Backend local DB settings — this is the file to edit if Postgres is different
$Api = Join-Path $Root "backend\OraEarn.Api"
$LocalExample = Join-Path $Api "appsettings.Local.json.example"
$LocalFile = Join-Path $Api "appsettings.Local.json"
if (-not (Test-Path $LocalFile) -and (Test-Path $LocalExample)) {
    Copy-Item $LocalExample $LocalFile
    Write-Ok "Created appsettings.Local.json — change Password here if your Postgres is not 12345"
}

Write-Step "Installing frontend packages..."
Push-Location $Frontend
npm install
Pop-Location
Write-Ok "Frontend ready"

# Backend
Write-Step "Restoring .NET packages..."
Push-Location $Api
dotnet restore
Pop-Location
Write-Ok "Backend ready"

# Aura (optional)
$Advisor = Join-Path $Root "ai-advisor"
$Venv = Join-Path $Advisor ".venv"
$Python = Get-Command python -ErrorAction SilentlyContinue
if ($Python) {
    if (-not (Test-Path $Venv)) {
        Write-Step "Creating Python venv for Aura..."
        Push-Location $Advisor
        python -m venv .venv
        Pop-Location
    }
    $Pip = Join-Path $Venv "Scripts\pip.exe"
    if (Test-Path $Pip) {
        & $Pip install -r (Join-Path $Advisor "requirements.txt") -q
        Write-Ok "Aura ready"
    }
} else {
    Write-Warn "Python not found — Aura skipped (optional)"
}

Write-Host ""
Write-Ok "Setup finished. Run each service in its own terminal:"
Write-Host "  cd backend;    .\run.ps1"
Write-Host "  cd frontend;   .\run.ps1"
Write-Host "  cd ai-advisor; .\run.ps1"
Write-Host ""
