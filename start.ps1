# Sobe o Postgres (+ pgAdmin) via Docker Compose e, quando o banco estiver pronto, roda a API.
# Uso: .\start.ps1

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir

Write-Host "Subindo containers (Postgres + pgAdmin)..." -ForegroundColor Cyan
docker-compose up -d

Write-Host "Aguardando o Postgres aceitar conexoes..." -ForegroundColor Cyan
$maxAttempts = 30
$attempt = 0
$ready = $false

while (-not $ready -and $attempt -lt $maxAttempts) {
    $attempt++
    docker exec fgames-postgres pg_isready -U fgames -d fgames *> $null
    if ($LASTEXITCODE -eq 0) {
        $ready = $true
    } else {
        Start-Sleep -Seconds 1
    }
}

if (-not $ready) {
    Write-Host "Postgres nao ficou pronto a tempo. Verifique 'docker ps' e 'docker logs fgames-postgres'." -ForegroundColor Red
    exit 1
}

Write-Host "Postgres pronto. Iniciando a API..." -ForegroundColor Green

