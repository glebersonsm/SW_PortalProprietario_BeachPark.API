# 🔄 Conversão de Encoding para UTF-8 with BOM

## ⚡ Quick Start (3 passos)

### 1️⃣ **Teste Primeiro** (Não altera nada)
```powershell
cd C:\SW_Solucoes\Projetos\SW_PortalProprietario_BeachPark.API
.\scripts\Test-Conversion.ps1
```

### 2️⃣ **Faça Commit de Segurança**
```bash
git add .
git commit -m "Checkpoint antes da conversão de encoding"
```

### 3️⃣ **Execute a Conversão**
```powershell
.\scripts\Convert-All-Files.ps1
```

---

## 📊 O que será convertido?

- ✅ **523 arquivos** (.cs, .json, .xml, .csproj, .sln, etc.)
- ✅ Encoding: **UTF-8 without BOM** → **UTF-8 with BOM**
- ✅ Backup automático antes de cada conversão
- ✅ Rollback automático se houver erro

---

## ⚠️ Checklist Pré-Conversão

- [ ] **Fechar Visual Studio** e outros editores
- [ ] **Fazer commit** das alterações pendentes no Git
- [ ] **Executar teste** (`Test-Conversion.ps1`) para ver o que será alterado
- [ ] **Verificar** que não há arquivos abertos/bloqueados

---

## ✅ Checklist Pós-Conversão

- [ ] **Verificar Git**: `git status` e `git diff`
- [ ] **Compilar**: `dotnet build`
- [ ] **Executar testes**: `dotnet test`
- [ ] **Commit**: `git commit -m "feat: Converter arquivos para UTF-8 with BOM"`

---

## 🎯 Por que UTF-8 with BOM?

| Problema | Antes (sem BOM) | Depois (com BOM) |
|----------|-----------------|------------------|
| Acentuação | ❌ Pode quebrar | ✅ Sempre correta |
| Visual Studio | ⚠️ Detecta como ASCII | ✅ Detecta como UTF-8 |
| Git diff | ⚠️ Confuso com caracteres especiais | ✅ Limpo |
| Cross-platform | ⚠️ Problemas no Linux/Mac | ✅ Funciona em todos |

---

## 📁 Arquivos Criados

```
scripts/
├── Convert-ToUtf8WithBOM.ps1      # ⚙️ Script principal completo
├── Convert-All-Files.ps1          # 🚀 Atalho rápido
├── Test-Conversion.ps1            # 🧪 Teste (não altera arquivos)
└── ENCODING_CONVERSION_GUIDE.md   # 📖 Documentação completa
.editorconfig                        # ⚙️ Configuração para novos arquivos
```

---

## 🔧 Configuração Permanente

O arquivo `.editorconfig` foi criado para garantir que **novos arquivos** usem UTF-8 BOM automaticamente.

**Visual Studio vai:**
- ✅ Detectar `.editorconfig` automaticamente
- ✅ Usar UTF-8 BOM em arquivos novos
- ✅ Manter formatação consistente

---

## 🆘 Ajuda Rápida

### Ver documentação completa
```powershell
notepad .\scripts\ENCODING_CONVERSION_GUIDE.md
```

### Reverter tudo (se necessário)
```bash
git checkout .
```

### Ver diferenças no Git
```bash
git diff --word-diff
```

### Converter pasta específica
```powershell
.\scripts\Convert-ToUtf8WithBOM.ps1 -Path "C:\Caminho\Especifico"
```

---

## 📞 Suporte

Leia o guia completo: `scripts\ENCODING_CONVERSION_GUIDE.md`

**Problemas comuns:**
- ❌ "Acesso negado" → Feche Visual Studio
- ❌ "Path not found" → Execute do diretório raiz do projeto
- ⚠️ Git mostra muitas alterações → Normal! É a mudança de encoding

---

**Pronto! Agora execute os 3 passos acima. ⬆️**
