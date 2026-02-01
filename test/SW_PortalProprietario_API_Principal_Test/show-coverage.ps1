# Script para mostrar o percentual de cobertura de testes
# Uso: .\show-coverage.ps1

param(
    [switch]$GenerateHtml
)

Write-Host "`n=== RESUMO DE COBERTURA DE TESTES ===" -ForegroundColor Cyan

# Procura o arquivo de cobertura mais recente
$coverageFile = Get-ChildItem -Path "coverage" -Filter "coverage.cobertura.xml" -Recurse | 
    Sort-Object LastWriteTime -Descending | 
    Select-Object -First 1

if (-not $coverageFile) {
    Write-Host "`nArquivo de cobertura não encontrado!" -ForegroundColor Red
    Write-Host "Execute os testes com cobertura primeiro:" -ForegroundColor Yellow
    Write-Host "  dotnet test SW_PortalProprietario.Test.csproj --collect:`"XPlat Code Coverage`" --results-directory:`"./coverage`"" -ForegroundColor Gray
    exit 1
}

try {
    [xml]$xml = Get-Content $coverageFile.FullName
    $summary = $xml.coverage
    
    if ($summary) {
        $lineRate = [math]::Round([double]$summary.'line-rate' * 100, 2)
        $branchRate = [math]::Round([double]$summary.'branch-rate' * 100, 2)
        $linesValid = [int]$summary.'lines-valid'
        $linesCovered = [int]$summary.'lines-covered'
        $branchesValid = [int]$summary.'branches-valid'
        $branchesCovered = [int]$summary.'branches-covered'
        
        Write-Host "`nCobertura de Linhas:" -ForegroundColor White
        Write-Host "  $lineRate% ($linesCovered de $linesValid linhas)" -ForegroundColor $(if ($lineRate -ge 70) { "Green" } elseif ($lineRate -ge 50) { "Yellow" } else { "Red" })
        
        Write-Host "`nCobertura de Branches:" -ForegroundColor White
        Write-Host "  $branchRate% ($branchesCovered de $branchesValid branches)" -ForegroundColor $(if ($branchRate -ge 70) { "Green" } elseif ($branchRate -ge 50) { "Yellow" } else { "Red" })
        
        Write-Host "`nArquivo de cobertura:" -ForegroundColor Gray
        Write-Host "  $($coverageFile.FullName)" -ForegroundColor DarkGray
        
        # Mostra cobertura por classe/pacote se disponível
        $packages = $xml.coverage.packages.package
        if ($packages) {
            Write-Host "`nCobertura por Pacote:" -ForegroundColor White
            foreach ($package in $packages) {
                $pkgLineRate = [math]::Round([double]$package.'line-rate' * 100, 2)
                $pkgName = $package.name
                Write-Host "  $pkgName : $pkgLineRate%" -ForegroundColor $(if ($pkgLineRate -ge 70) { "Green" } elseif ($pkgLineRate -ge 50) { "Yellow" } else { "Red" })
            }
        }
        
        # Gera relatório HTML se solicitado
        if ($GenerateHtml) {
            $reportGenInstalled = dotnet tool list -g | Select-String "reportgenerator"
            
            if ($reportGenInstalled) {
                Write-Host "`nGerando relatório HTML..." -ForegroundColor Cyan
                $htmlDir = "coverage/html"
                New-Item -ItemType Directory -Force -Path $htmlDir | Out-Null
                
                reportgenerator `
                    -reports:"$($coverageFile.FullName)" `
                    -targetdir:"$htmlDir" `
                    -reporttypes:"Html;Badges"
                
                Write-Host "`nRelatório HTML gerado em: $htmlDir/index.html" -ForegroundColor Green
                Write-Host "Abra o arquivo no navegador para visualizar a cobertura detalhada." -ForegroundColor Yellow
            } else {
                Write-Host "`nReportGenerator não está instalado." -ForegroundColor Yellow
                Write-Host "Para gerar relatório HTML, instale:" -ForegroundColor Yellow
                Write-Host "  dotnet tool install -g dotnet-reportgenerator-globaltool" -ForegroundColor Cyan
            }
        }
    } else {
        Write-Host "`nNão foi possível ler o resumo de cobertura do arquivo." -ForegroundColor Red
    }
} catch {
    Write-Host "`nErro ao processar arquivo de cobertura: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""

