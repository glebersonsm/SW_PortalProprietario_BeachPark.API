# Script para converter arquivos para UTF-8 with BOM
# Uso: .\Convert-ToUtf8WithBOM.ps1 [-Path "C:\Caminho\Projeto"] [-WhatIf] [-Verbose]

[CmdletBinding(SupportsShouldProcess)]
param(
    [Parameter(Mandatory=$false)]
    [string]$Path = (Get-Location).Path
)

# Extensões de arquivo a serem convertidas
$extensionsToConvert = @(
    '*.cs',      # C# source files
    '*.csproj',  # Project files
    '*.sln',     # Solution files
    '*.json',    # JSON files (appsettings, etc)
    '*.xml',     # XML files
    '*.config',  # Config files
    '*.resx',    # Resource files
    '*.txt',     # Text files
    '*.md',      # Markdown files
    '*.cshtml',  # Razor views
    '*.razor',   # Razor components
    '*.css',     # CSS files
    '*.js',      # JavaScript files
    '*.ts',      # TypeScript files
    '*.sql'      # SQL files
)

# Pastas a serem excluídas
$excludedFolders = @(
    'bin',
    'obj',
    'packages',
    '.vs',
    '.git',
    'node_modules',
    'TestResults',
    'Properties\PublishProfiles'
)

# UTF-8 with BOM encoding
$utf8WithBom = New-Object System.Text.UTF8Encoding $true

# Estatísticas
$stats = @{
    TotalFiles = 0
    ConvertedFiles = 0
    SkippedFiles = 0
    ErrorFiles = 0
    AlreadyUtf8WithBom = 0
}

function Test-IsExcludedPath {
    param([string]$FilePath)
    
    foreach ($excluded in $excludedFolders) {
        if ($FilePath -like "*\$excluded\*") {
            return $true
        }
    }
    return $false
}

function Get-FileEncoding {
    param([string]$FilePath)
    
    try {
        $bytes = [System.IO.File]::ReadAllBytes($FilePath)
        
        # Check for UTF-8 BOM (EF BB BF)
        if ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF) {
            return 'UTF8-BOM'
        }
        
        # Check for UTF-16 LE BOM (FF FE)
        if ($bytes.Length -ge 2 -and $bytes[0] -eq 0xFF -and $bytes[1] -eq 0xFE) {
            return 'UTF16-LE'
        }
        
        # Check for UTF-16 BE BOM (FE FF)
        if ($bytes.Length -ge 2 -and $bytes[0] -eq 0xFE -and $bytes[1] -eq 0xFF) {
            return 'UTF16-BE'
        }
        
        # No BOM - likely UTF-8 without BOM or ASCII
        return 'UTF8-NoBOM-or-ASCII'
    }
    catch {
        return 'UNKNOWN'
    }
}

function Convert-FileToUtf8WithBom {
param(
    [string]$FilePath
)
    
try {
    # Detectar encoding atual
    $currentEncoding = Get-FileEncoding -FilePath $FilePath
        
    if ($currentEncoding -eq 'UTF8-BOM') {
        if ($VerbosePreference -eq 'Continue') {
            Write-Host "  [SKIP] Já está em UTF-8 with BOM: $FilePath" -ForegroundColor Gray
        }
        $stats.AlreadyUtf8WithBom++
        return $true
    }
        
    if ($WhatIfPreference) {
        Write-Host "  [WHAT-IF] Converteria: $FilePath (Encoding atual: $currentEncoding)" -ForegroundColor Cyan
        $stats.ConvertedFiles++
        return $true
    }
        
        # Ler conteúdo com detecção automática de encoding
        $content = Get-Content -Path $FilePath -Raw
        
        # Fazer backup temporário
        $backupPath = "$FilePath.backup"
        Copy-Item -Path $FilePath -Destination $backupPath -Force
        
        try {
            # Escrever com UTF-8 BOM
            [System.IO.File]::WriteAllText($FilePath, $content, $utf8WithBom)
            
            # Verificar se a conversão foi bem-sucedida
            $newEncoding = Get-FileEncoding -FilePath $FilePath
            if ($newEncoding -eq 'UTF8-BOM') {
                Remove-Item -Path $backupPath -Force
                Write-Host "  [OK] Convertido: $FilePath (de $currentEncoding para UTF-8 BOM)" -ForegroundColor Green
                $stats.ConvertedFiles++
                return $true
            }
            else {
                # Restaurar backup se algo deu errado
                Move-Item -Path $backupPath -Destination $FilePath -Force
                Write-Warning "  [ERRO] Falha na conversão: $FilePath"
                $stats.ErrorFiles++
                return $false
            }
        }
        catch {
            # Restaurar backup em caso de erro
            if (Test-Path $backupPath) {
                Move-Item -Path $backupPath -Destination $FilePath -Force
            }
            throw
        }
    }
    catch {
        Write-Warning "  [ERRO] Não foi possível converter: $FilePath - $_"
        $stats.ErrorFiles++
        return $false
    }
}

# Main execution
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Conversão de arquivos para UTF-8 with BOM" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Pasta raiz: $Path" -ForegroundColor Yellow
Write-Host "Modo: $(if ($WhatIfPreference) { 'SIMULAÇÃO (WhatIf)' } else { 'EXECUÇÃO REAL' })" -ForegroundColor $(if ($WhatIfPreference) { 'Cyan' } else { 'Red' })
Write-Host ""

if (-not (Test-Path $Path)) {
    Write-Error "Caminho não existe: $Path"
    exit 1
}

Write-Host "Buscando arquivos..." -ForegroundColor Yellow

# Buscar todos os arquivos com as extensões especificadas
$allFiles = @()
foreach ($extension in $extensionsToConvert) {
    $files = Get-ChildItem -Path $Path -Filter $extension -Recurse -File -ErrorAction SilentlyContinue
    $allFiles += $files
}

# Filtrar arquivos excluídos
$filesToProcess = $allFiles | Where-Object { -not (Test-IsExcludedPath $_.FullName) }

$stats.TotalFiles = $filesToProcess.Count

Write-Host "Arquivos encontrados: $($stats.TotalFiles)" -ForegroundColor Yellow
Write-Host ""

if ($stats.TotalFiles -eq 0) {
    Write-Host "Nenhum arquivo encontrado para processar." -ForegroundColor Yellow
    exit 0
}

# Processar arquivos
$counter = 0
foreach ($file in $filesToProcess) {
    $counter++
    $percentComplete = [math]::Round(($counter / $stats.TotalFiles) * 100, 2)
    
    Write-Progress -Activity "Convertendo arquivos para UTF-8 with BOM" `
                   -Status "Processando: $($file.Name)" `
                   -PercentComplete $percentComplete `
                   -CurrentOperation "$counter de $($stats.TotalFiles)"
    
    if ($VerbosePreference -eq 'Continue') {
        Write-Host "[$counter/$($stats.TotalFiles)] Processando: $($file.FullName)" -ForegroundColor White
    }
    
    Convert-FileToUtf8WithBom -FilePath $file.FullName
}

Write-Progress -Activity "Convertendo arquivos para UTF-8 with BOM" -Completed

# Exibir estatísticas
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Resumo da Conversão" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Total de arquivos encontrados: $($stats.TotalFiles)" -ForegroundColor White
Write-Host "Já estavam em UTF-8 BOM:       $($stats.AlreadyUtf8WithBom)" -ForegroundColor Gray
Write-Host "Arquivos convertidos:          $($stats.ConvertedFiles)" -ForegroundColor Green
Write-Host "Erros:                         $($stats.ErrorFiles)" -ForegroundColor $(if ($stats.ErrorFiles -gt 0) { 'Red' } else { 'Gray' })
Write-Host ""

if ($WhatIfPreference) {
    Write-Host "ATENÇÃO: Este foi um modo de SIMULAÇÃO (WhatIf)." -ForegroundColor Cyan
    Write-Host "Execute novamente SEM o parâmetro -WhatIf para aplicar as mudanças." -ForegroundColor Cyan
}
else {
    Write-Host "Conversão concluída!" -ForegroundColor Green
}

# Retornar código de saída
if ($stats.ErrorFiles -gt 0) {
    exit 1
}
else {
    exit 0
}
