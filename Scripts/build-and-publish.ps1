# Build and Publish Script for Windows
# build-and-publish.ps1

Write-Host "?? Iniciando build do projeto..." -ForegroundColor Cyan

# Definir variáveis
$ProjectPath = ".\SW_PortalCliente_BeachPark.API.csproj"
$OutputPath = ".\publish"
$Configuration = "Release"

# Limpar build anterior
Write-Host "?? Limpando build anterior..." -ForegroundColor Yellow
if (Test-Path $OutputPath) {
    Remove-Item -Path $OutputPath -Recurse -Force
}
dotnet clean

# Restaurar dependências
Write-Host "?? Restaurando dependências..." -ForegroundColor Yellow
dotnet restore

# Build do projeto
Write-Host "?? Compilando projeto..." -ForegroundColor Yellow
dotnet build $ProjectPath -c $Configuration --no-restore

# Publicar
Write-Host "?? Publicando aplicação..." -ForegroundColor Yellow
dotnet publish $ProjectPath `
  -c $Configuration `
  -o $OutputPath `
  --no-build `
  --self-contained false `
  /p:PublishSingleFile=false `
  /p:PublishTrimmed=false

# Copiar arquivo .env de produção
Write-Host "?? Copiando arquivo de configuração..." -ForegroundColor Yellow
if (Test-Path ".env.production") {
    Copy-Item ".env.production" -Destination "$OutputPath\.env"
    Write-Host "? Arquivo .env.production copiado" -ForegroundColor Green
} else {
    Write-Host "??  Aviso: Arquivo .env.production não encontrado" -ForegroundColor Yellow
}

Write-Host "? Build e publicação concluídos com sucesso!" -ForegroundColor Green
Write-Host "?? Arquivos publicados em: $OutputPath" -ForegroundColor Cyan
