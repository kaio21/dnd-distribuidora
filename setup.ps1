# D&D Distribuidora - Script de Setup
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  D&D Distribuidora - Setup Inicial" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

Write-Host "`n[1/3] Restaurando pacotes do backend..." -ForegroundColor Yellow
Set-Location "$PSScriptRoot\DnD.API"
dotnet restore
if ($LASTEXITCODE -ne 0) { Write-Host "Erro no restore do backend!" -ForegroundColor Red; exit 1 }

Write-Host "`n[2/3] Criando migrations e aplicando ao banco..." -ForegroundColor Yellow
dotnet ef migrations add InitialCreate 2>$null
dotnet ef database update
if ($LASTEXITCODE -ne 0) {
    Write-Host "Erro nas migrations. Verifique a connection string no appsettings.json" -ForegroundColor Red
    Write-Host "Alternativa: execute o script Migrations\InitialCreate.sql manualmente no SQL Server" -ForegroundColor Yellow
}

Write-Host "`n[3/3] Instalando dependencias do frontend..." -ForegroundColor Yellow
Set-Location "$PSScriptRoot\dnd-frontend"
npm install
if ($LASTEXITCODE -ne 0) { Write-Host "Erro no npm install!" -ForegroundColor Red; exit 1 }

Write-Host "`n========================================" -ForegroundColor Green
Write-Host "  Setup concluido!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host "`nPara iniciar o projeto, abra 2 terminais:" -ForegroundColor White
Write-Host "  Terminal 1 (Backend):  cd DnD.API && dotnet run" -ForegroundColor Cyan
Write-Host "  Terminal 2 (Frontend): cd dnd-frontend && npm run dev" -ForegroundColor Cyan
Write-Host "`nAcesse: http://localhost:5173" -ForegroundColor Green
Write-Host "API Swagger: http://localhost:5000/swagger" -ForegroundColor Green
