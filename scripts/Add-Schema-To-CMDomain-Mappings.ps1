# Script para adicionar Schema("cm"); em todos os mappings do CMDomain

$mappingsPath = "CMDomain\Mappings"
$files = Get-ChildItem -Path $mappingsPath -Filter "*Map.cs" -File

$stats = @{
    Total = 0
    Updated = 0
    AlreadyHas = 0
    Errors = 0
}

Write-Host "Verificando arquivos de mapeamento..." -ForegroundColor Cyan
Write-Host ""

foreach ($file in $files) {
    $stats.Total++
    
    try {
        $content = Get-Content $file.FullName -Raw
        
        # Verifica se já tem Schema
        if ($content -match 'Schema\s*\(\s*"cm"\s*\)') {
            Write-Host "  [SKIP] Já tem Schema: $($file.Name)" -ForegroundColor Gray
            $stats.AlreadyHas++
            continue
        }
        
        # Verifica se tem Table(
        if ($content -notmatch 'Table\s*\(') {
            Write-Host "  [WARN] Sem Table(): $($file.Name)" -ForegroundColor Yellow
            continue
        }
        
        # Adiciona Schema("cm"); após a última linha de Table
        # Padrão: após Table("NomeTabela"); adicionar nova linha com Schema("cm");
        $newContent = $content -replace '(Table\s*\(\s*"[^"]+"\s*\)\s*;)', "`$1`r`n            Schema(`"cm`");"
        
        # Salvar
        Set-Content -Path $file.FullName -Value $newContent -NoNewline
        
        Write-Host "  [OK] Adicionado Schema: $($file.Name)" -ForegroundColor Green
        $stats.Updated++
    }
    catch {
        Write-Host "  [ERRO] $($file.Name): $_" -ForegroundColor Red
        $stats.Errors++
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Resumo" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Total:              $($stats.Total)"
Write-Host "Já tinham Schema:   $($stats.AlreadyHas)" -ForegroundColor Gray
Write-Host "Adicionados:        $($stats.Updated)" -ForegroundColor Green
Write-Host "Erros:              $($stats.Errors)" -ForegroundColor $(if ($stats.Errors -gt 0) { 'Red' } else { 'Gray' })
