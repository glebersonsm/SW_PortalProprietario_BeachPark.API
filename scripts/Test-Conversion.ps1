# Script para testar a conversão (modo WhatIf - não altera arquivos)
# Mostra o que SERIA alterado sem fazer mudanças

$scriptPath = Join-Path $PSScriptRoot "Convert-ToUtf8WithBOM.ps1"
$projectRoot = Split-Path $PSScriptRoot -Parent

Write-Host "MODO DE TESTE - Nenhum arquivo será alterado" -ForegroundColor Cyan
Write-Host "Este script mostra o que SERIA convertido" -ForegroundColor Cyan
Write-Host ""

& $scriptPath -Path $projectRoot -WhatIf

Write-Host ""
Write-Host "Para executar a conversão REAL, execute: Convert-All-Files.ps1" -ForegroundColor Yellow
Write-Host ""
Write-Host "Pressione qualquer tecla para continuar..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
