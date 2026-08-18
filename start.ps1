# Sobe Postgres + pgAdmin + Seq via Docker Compose e, quando as dependencias obrigatorias
# estiverem prontas, roda a API.
# Uso: .\start.ps1

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir

Write-Host "Subindo containers (Postgres + pgAdmin + Seq)..." -ForegroundColor Cyan
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

Write-Host "Postgres pronto." -ForegroundColor Green

Write-Host "Aguardando o Seq aceitar conexoes..." -ForegroundColor Cyan
$seqMaxAttempts = 30
$seqAttempt = 0
$seqReady = $false

while (-not $seqReady -and $seqAttempt -lt $seqMaxAttempts) {
    $seqAttempt++
    try {
        Invoke-WebRequest -Uri "http://localhost:5342" -UseBasicParsing -TimeoutSec 2 | Out-Null
        $seqReady = $true
    } catch {
        Start-Sleep -Seconds 1
    }
}

if (-not $seqReady) {
    # Nao bloqueia a subida: o sink do Seq no Serilog reenvia os eventos quando o servico ficar disponivel.
    Write-Host "Seq nao respondeu a tempo (nao critico). Verifique 'docker logs fgames-seq' se os logs nao aparecerem em http://localhost:8082." -ForegroundColor Yellow
} else {
    Write-Host "Seq pronto (UI em http://localhost:8082)." -ForegroundColor Green
}

Write-Host "Iniciando a API..." -ForegroundColor Green
dotnet run --project src/Host/FGames.Api

