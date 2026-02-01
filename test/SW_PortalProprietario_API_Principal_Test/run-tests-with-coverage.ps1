# Script para executar testes com cobertura
# Uso: .\run-tests-with-coverage.ps1

Write-Host "Executando testes com cobertura..." -ForegroundColor Cyan

$testProjectPath = "SW_PortalProprietario.Test.csproj"
$coverageOutput = "coverage"
$coverageFormat = "opencover"

# Executa os testes com cobertura
dotnet test $testProjectPath `
    /p:CollectCoverage=true `
    /p:CoverletOutputFormat=$coverageFormat `
    /p:CoverletOutput="./$coverageOutput/" `
    /p:ExcludeByFile="**/Program.cs,**/Migrations/**" `
    /p:ExcludeByAttribute="Obsolete,GeneratedCodeAttribute,CompilerGeneratedAttribute" `
    /p:Exclude="[*]*.Migrations.*"

if ($LASTEXITCODE -eq 0) {
    Write-Host "`nTestes executados com sucesso!" -ForegroundColor Green
    Write-Host "`nArquivos de cobertura gerados em: $coverageOutput/" -ForegroundColor Yellow
    
    # Verifica se o ReportGenerator está instalado
    $reportGenInstalled = dotnet tool list -g | Select-String "reportgenerator"
    
    if ($reportGenInstalled) {
        Write-Host "`nGerando relatório HTML..." -ForegroundColor Cyan
        $coverageFile = Get-ChildItem -Path $coverageOutput -Filter "coverage.opencover.xml" -Recurse | Select-Object -First 1
        
        if ($coverageFile) {
            reportgenerator `
                -reports:"$($coverageFile.FullName)" `
                -targetdir:"$coverageOutput/html" `
                -reporttypes:"Html;Badges"
            
            Write-Host "`nRelatório HTML gerado em: $coverageOutput/html/index.html" -ForegroundColor Green
            Write-Host "Abra o arquivo no navegador para visualizar a cobertura detalhada." -ForegroundColor Yellow
        }
    } else {
        Write-Host "`nPara gerar relatório HTML, instale o ReportGenerator:" -ForegroundColor Yellow
        Write-Host "dotnet tool install -g dotnet-reportgenerator-globaltool" -ForegroundColor Cyan
        Write-Host "`nOu visualize o arquivo XML diretamente em: $coverageOutput/coverage.opencover.xml" -ForegroundColor Yellow
    }
    
    # Mostra resumo da cobertura do arquivo XML
    $coverageFile = Get-ChildItem -Path $coverageOutput -Filter "coverage.opencover.xml" -Recurse | Select-Object -First 1
    if ($coverageFile) {
        Write-Host "`nResumo da cobertura:" -ForegroundColor Cyan
        $xml = [xml](Get-Content $coverageFile.FullName)
        $summary = $xml.SelectSingleNode("//Summary")
        if ($summary) {
            $lineRate = [math]::Round([double]$summary.linecoverage * 100, 2)
            $branchRate = [math]::Round([double]$summary.branchcoverage * 100, 2)
            Write-Host "  Linhas: $lineRate%" -ForegroundColor $(if ($lineRate -ge 70) { "Green" } else { "Yellow" })
            Write-Host "  Branches: $branchRate%" -ForegroundColor $(if ($branchRate -ge 70) { "Green" } else { "Yellow" })
        }
    }
} else {
    Write-Host "`nErro ao executar os testes!" -ForegroundColor Red
    exit $LASTEXITCODE
}

