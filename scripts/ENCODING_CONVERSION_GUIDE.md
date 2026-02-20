# Guia de Uso - Conversão de Arquivos para UTF-8 with BOM

## ?? Visão Geral

Este conjunto de scripts converte todos os arquivos do projeto para **UTF-8 with BOM** (Byte Order Mark).

## ?? Por que UTF-8 with BOM?

### Vantagens
- ? **Compatibilidade:** Reconhecido por todos os editores e IDEs modernos
- ? **Acentuação:** Garante exibição correta de caracteres especiais (português, etc.)
- ? **Visual Studio:** Detecta automaticamente o encoding
- ? **Git:** Evita problemas de diff com caracteres especiais
- ? **Cross-platform:** Funciona em Windows, Linux e macOS

### O que é BOM?
BOM (Byte Order Mark) são 3 bytes no início do arquivo (`EF BB BF`) que indicam explicitamente que o arquivo é UTF-8.

## ?? Arquivos do Script

```
scripts/
??? Convert-ToUtf8WithBOM.ps1      # Script principal (completo)
??? Convert-All-Files.ps1          # Atalho para conversão rápida
??? Test-Conversion.ps1            # Teste (modo WhatIf)
??? ENCODING_CONVERSION_GUIDE.md   # Este arquivo
```

## ?? Como Usar

### Opção 1: Teste Primeiro (Recomendado)

**Não altera nenhum arquivo**, apenas mostra o que seria convertido:

```powershell
cd C:\SW_Solucoes\Projetos\SW_PortalProprietario_BeachPark.API
.\scripts\Test-Conversion.ps1
```

**Saída esperada:**
```
MODO DE TESTE - Nenhum arquivo será alterado
Este script mostra o que SERIA convertido

Pasta raiz: C:\SW_Solucoes\Projetos\SW_PortalProprietario_BeachPark.API
Modo: SIMULAÇÃO (WhatIf)

Buscando arquivos...
Arquivos encontrados: 523

[WHAT-IF] Converteria: C:\...\AuthController.cs (Encoding atual: UTF8-NoBOM-or-ASCII)
[WHAT-IF] Converteria: C:\...\Program.cs (Encoding atual: UTF8-NoBOM-or-ASCII)
...

========================================
Resumo da Conversão
========================================
Total de arquivos encontrados: 523
Já estavam em UTF-8 BOM:       45
Arquivos convertidos:          478
Erros:                         0
```

### Opção 2: Conversão Completa

**Converte todos os arquivos** do projeto:

```powershell
cd C:\SW_Solucoes\Projetos\SW_PortalProprietario_BeachPark.API
.\scripts\Convert-All-Files.ps1
```

O script vai:
1. Pedir confirmação
2. Criar backup automático de cada arquivo antes de converter
3. Restaurar backup em caso de erro
4. Mostrar progresso com barra de status
5. Exibir resumo ao final

### Opção 3: Uso Avançado (Script Principal)

Para uso avançado com parâmetros customizados:

```powershell
# Converter pasta específica
.\scripts\Convert-ToUtf8WithBOM.ps1 -Path "C:\Caminho\Especifico"

# Modo teste (WhatIf)
.\scripts\Convert-ToUtf8WithBOM.ps1 -WhatIf

# Com logs detalhados
.\scripts\Convert-ToUtf8WithBOM.ps1 -Verbose

# Combinando parâmetros
.\scripts\Convert-ToUtf8WithBOM.ps1 -Path "C:\Projeto" -WhatIf -Verbose
```

## ?? Tipos de Arquivo Convertidos

O script converte automaticamente:

| Tipo | Extensões |
|------|-----------|
| **C# e .NET** | `.cs`, `.csproj`, `.sln`, `.config`, `.resx` |
| **Configuração** | `.json`, `.xml`, `.txt` |
| **Web** | `.cshtml`, `.razor`, `.css`, `.js`, `.ts` |
| **Documentação** | `.md` |
| **Banco de Dados** | `.sql` |

## ?? Pastas Excluídas

O script **ignora automaticamente** estas pastas:
- `bin/` - Binários compilados
- `obj/` - Objetos intermediários
- `packages/` - Pacotes NuGet
- `.vs/` - Configurações do Visual Studio
- `.git/` - Repositório Git
- `node_modules/` - Dependências Node.js
- `TestResults/` - Resultados de testes

## ?? Detecção de Encoding

O script detecta automaticamente o encoding atual:

| Encoding Detectado | BOM | Ação |
|--------------------|-----|------|
| **UTF-8 with BOM** | `EF BB BF` | ? Já correto, pula |
| **UTF-8 without BOM** | Nenhum | ?? Converte |
| **ASCII** | Nenhum | ?? Converte |
| **UTF-16 LE** | `FF FE` | ?? Converte |
| **UTF-16 BE** | `FE FF` | ?? Converte |

## ??? Segurança e Backup

### Backup Automático
- Cada arquivo recebe um backup temporário (`.backup`)
- Se a conversão falhar, o backup é restaurado automaticamente
- Se a conversão for bem-sucedida, o backup é removido

### Verificação Pós-Conversão
Após cada conversão, o script:
1. Lê o arquivo novamente
2. Verifica se o BOM UTF-8 está presente
3. Se não estiver correto, restaura o backup

### Rollback Manual
Se precisar desfazer tudo:

```powershell
# Usar o Git para reverter
git checkout .

# Ou reverter arquivos específicos
git checkout -- "caminho/do/arquivo.cs"
```

## ?? Exemplo de Saída Completa

```
========================================
Conversão de arquivos para UTF-8 with BOM
========================================
Pasta raiz: C:\SW_Solucoes\Projetos\SW_PortalProprietario_BeachPark.API
Modo: EXECUÇÃO REAL

Buscando arquivos...
Arquivos encontrados: 523

Processando: AuthController.cs (1/523)
  [OK] Convertido: AuthController.cs (de UTF8-NoBOM-or-ASCII para UTF-8 BOM)
  
Processando: Program.cs (2/523)
  [SKIP] Já está em UTF-8 with BOM: Program.cs
  
Processando: appsettings.json (3/523)
  [OK] Convertido: appsettings.json (de UTF8-NoBOM-or-ASCII para UTF-8 BOM)

... (processando todos os arquivos)

========================================
Resumo da Conversão
========================================
Total de arquivos encontrados: 523
Já estavam em UTF-8 BOM:       45
Arquivos convertidos:          478
Erros:                         0

Conversão concluída!
```

## ?? Avisos Importantes

### Antes de Executar

1. **Faça commit** das alterações pendentes no Git:
   ```bash
   git add .
   git commit -m "Checkpoint antes da conversão de encoding"
   ```

2. **Feche o Visual Studio** e outros editores:
   - Evita conflitos de arquivo aberto
   - Permite que o script acesse todos os arquivos

3. **Execute teste primeiro**:
   ```powershell
   .\scripts\Test-Conversion.ps1
   ```

### Após Executar

1. **Verifique no Git** as mudanças:
   ```bash
   git status
   git diff
   ```

2. **Compile o projeto**:
   ```bash
   dotnet build
   ```

3. **Execute testes**:
   ```bash
   dotnet test
   ```

4. **Commit das mudanças**:
   ```bash
   git add .
   git commit -m "feat: Converter todos os arquivos para UTF-8 with BOM"
   ```

## ?? Troubleshooting

### Erro: "Acesso negado"
**Causa:** Arquivo aberto em outro programa  
**Solução:** Feche Visual Studio, IIS, e outros processos que podem ter arquivos abertos

### Erro: "Path not found"
**Causa:** Script executado do diretório errado  
**Solução:** 
```powershell
cd C:\SW_Solucoes\Projetos\SW_PortalProprietario_BeachPark.API
.\scripts\Convert-All-Files.ps1
```

### Muitos erros na conversão
**Causa:** Arquivos binários ou corrompidos  
**Solução:** Verifique os arquivos com erro manualmente, podem ser arquivos que não deveriam ser texto

### Git mostra alterações em todos os arquivos
**Causa:** Mudança de encoding (esperado!)  
**Solução:** Isso é normal. O Git detecta mudança no BOM. Verifique com:
```bash
git diff --word-diff
```

## ?? Referências

- [UTF-8 BOM - Wikipedia](https://en.wikipedia.org/wiki/Byte_order_mark#UTF-8)
- [.NET Encoding Class](https://docs.microsoft.com/en-us/dotnet/api/system.text.encoding)
- [Visual Studio - File Encoding](https://docs.microsoft.com/en-us/visualstudio/ide/encodings-and-line-breaks)

## ? FAQ

**P: Preciso converter novamente no futuro?**  
R: Não. O Visual Studio manterá UTF-8 BOM em arquivos novos se estiver configurado.

**P: Afeta o tamanho dos arquivos?**  
R: Sim, mas mínimo. Cada arquivo ganha apenas 3 bytes (BOM).

**P: Funcionará no Linux/macOS?**  
R: Sim! UTF-8 BOM é multiplataforma.

**P: Posso executar em CI/CD?**  
R: Sim, o script aceita parâmetros não-interativos:
```powershell
.\Convert-ToUtf8WithBOM.ps1 -Path $env:BUILD_SOURCESDIRECTORY -Confirm:$false
```

**P: E se eu quiser UTF-8 SEM BOM?**  
R: Modifique a linha no script:
```powershell
$utf8WithBom = New-Object System.Text.UTF8Encoding $false  # Remove BOM
```

## ?? Configurar Visual Studio

Para garantir que novos arquivos usem UTF-8 BOM:

1. **Tools ? Options**
2. **Environment ? Documents**
3. Marque: **"Save documents as Unicode (UTF-8 with signature) when data cannot be saved in codepage"**

Ou edite `.editorconfig`:
```ini
[*]
charset = utf-8-bom
```

---

**Pronto para converter?**
```powershell
# 1. Teste primeiro
.\scripts\Test-Conversion.ps1

# 2. Se tudo ok, execute
.\scripts\Convert-All-Files.ps1
```
