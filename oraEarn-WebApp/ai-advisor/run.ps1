$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot
if (-not (Test-Path ".venv")) {
    python -m venv .venv
    & .\.venv\Scripts\pip.exe install -r requirements.txt
}
& .\.venv\Scripts\uvicorn.exe main:app --reload --port 8000
