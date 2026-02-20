# Script rápido para converter todos os arquivos do projeto atual
# Executa o script principal com parâmetros padrão

$scriptPath = Join-Path $PSScriptRoot "Convert-ToUtf8WithBOM.ps1"
$projectRoot = Split-Path $PSScriptRoot -Parent

Write-Host "Convertendo todos os arquivos do projeto para UTF-8 with BOM..." -ForegroundColor Cyan
Write-Host "Pasta do projeto: $projectRoot" -ForegroundColor Yellow
Write-Host ""

# Perguntar confirmação
$confirmation = Read-Host "Deseja continuar? (S/N)"
if ($confirmation -ne 'S' -and $confirmation -ne 's') {
    Write-Host "Operação cancelada pelo usuário." -ForegroundColor Yellow
    exit 0
}

# Executar conversão
& $scriptPath -Path $projectRoot

Write-Host ""
Write-Host "Pressione qualquer tecla para continuar..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
