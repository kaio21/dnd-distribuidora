Set-Location "$PSScriptRoot\DnD.API"
Write-Host "Iniciando Backend .NET na porta 5000..." -ForegroundColor Green
$env:ASPNETCORE_URLS = "http://localhost:5000"
dotnet run
