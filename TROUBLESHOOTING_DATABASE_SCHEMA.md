# Troubleshooting - Criação de Schema no PostgreSQL

## Problema
As tabelas não estão sendo geradas no schema `portalohana` no banco de dados PostgreSQL.

## Solução Implementada

### 1. Logs de Erro Habilitados

Foi adicionado logging detalhado no método `BuildSchema` da classe `NHibernateExtensions.cs` para capturar e exibir erros durante a criação/atualização do schema.

### 2. Configurações Adicionadas

As seguintes configurações foram habilitadas para PostgreSQL:

```csharp
.ShowSql()           // Exibir SQL gerado pelo NHibernate
.FormatSql()         // Formatar SQL para melhor legibilidade
.Raw("throw_on_error", "true")  // Lançar exceções em erros
```

### 3. Schema Padrão Configurado

```csharp
.Raw("default_schema", "portalohana")  // Define o schema padrão
```

## Como Ver os Logs de Erro

### Opção 1: Console do Aplicativo

Ao iniciar a aplicação, os logs serão exibidos no console com o seguinte formato:

```
========================================
INICIANDO ATUALIZAÇÃO DO SCHEMA NO POSTGRESQL
========================================
[SQL] CREATE TABLE portalohana.usuario (...)
[SQL] CREATE TABLE portalohana.pessoa (...)
...
========================================
SCHEMA ATUALIZADO COM SUCESSO!
========================================
```

**EM CASO DE ERRO:**

```
========================================
ERROS ENCONTRADOS DURANTE A ATUALIZAÇÃO DO SCHEMA:
========================================
ERRO: permission denied for schema portalohana
STACK TRACE: ...
----------------------------------------
========================================
```

### Opção 2: Visual Studio Output Window

1. Abra o Visual Studio
2. Vá em **View ? Output** (ou pressione `Ctrl+Alt+O`)
3. Na janela Output, selecione "Debug" no dropdown
4. Execute a aplicação e observe os logs

### Opção 3: Logs do NLog (Se Configurado)

Se você tiver o NLog configurado, os logs também serão gravados nos arquivos de log configurados.

## Verificações Importantes

### 1. Verificar Configuração do appsettings.json

Certifique-se de que a propriedade `UpdateDataBase` está definida como `true`:

```json
{
  "UpdateDataBase": true
}
```

**Localização possível:**
- `appsettings.json`
- `appsettings.Development.json`
- `SourceConfiguration\BeachParkConfigurations.json`

### 2. Verificar Variável de Ambiente

Certifique-se de que a variável de ambiente `DEFAULT_CONNECTION` está configurada corretamente:

```
DEFAULT_CONNECTION=Host=seu_host;PORT=5432;Database=seu_banco;Username=seu_usuario;Password=sua_senha
```

**Verificar no arquivo `.env`:**

```env
DEFAULT_CONNECTION=Host=localhost;PORT=5432;Database=portaldb;Username=postgres;Password=senha123
```

### 3. Verificar Permissões do Banco de Dados

O usuário do banco de dados precisa ter permissões para:

```sql
-- Verificar se o schema existe
SELECT schema_name 
FROM information_schema.schemata 
WHERE schema_name = 'portalohana';

-- Se não existir, criar:
CREATE SCHEMA IF NOT EXISTS portalohana;

-- Dar permissões ao usuário:
GRANT ALL PRIVILEGES ON SCHEMA portalohana TO seu_usuario;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA portalohana TO seu_usuario;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA portalohana TO seu_usuario;

-- Configurar search_path (opcional):
ALTER USER seu_usuario SET search_path TO portalohana, public;
```

### 4. Testar Conexão com o Banco

Execute o seguinte script SQL para verificar se a conexão está funcionando:

```sql
-- Verificar schema atual
SELECT current_schema();

-- Listar todos os schemas
SELECT schema_name 
FROM information_schema.schemata 
ORDER BY schema_name;

-- Verificar permissões do usuário
SELECT * 
FROM information_schema.role_table_grants 
WHERE grantee = 'seu_usuario';
```

## Erros Comuns e Soluções

### Erro: "Dialect does not support DbType.Guid"

**Descrição Completa:**
```
ERRO: Dialect does not support DbType.Guid (Parameter 'typecode')
STACK TRACE:    at NHibernate.Dialect.TypeNames.Get(DbType typecode)
   at NHibernate.Dialect.Dialect.GetTypeName(SqlType sqlType)
```

**Causa:** O PostgreSQL não suporta nativamente o tipo `DbType.Guid` do .NET. O PostgreSQL usa o tipo `UUID` ao invés de `GUID`. Este erro ocorre quando:

1. Alguma entidade tem propriedade `virtual Guid NomePropriedade` (tipo `System.Guid`)
2. O NHibernate está tentando fazer automapping dessa propriedade
3. A entidade não tem um mapeamento FluentNHibernate explícito

**Solução Implementada:**

? **Todas as entidades agora usam `string?` para IDs/GUIDs:**
```csharp
// ? CORRETO - Usar string
public virtual string? ObjectGuid { get; set; }
public virtual string? SagaId { get; set; }

// ? ERRADO - NÃO usar System.Guid
public virtual Guid ObjectGuid { get; set; }  // Causa erro!
```

? **Criados mapeamentos para `SagaExecution` e `SagaStep`:**
- `SW_PortalProprietario.Infra.Data\Mappings\Core\Sistema\SagaExecutionMap.cs`
- `SW_PortalProprietario.Infra.Data\Mappings\Core\Sistema\SagaStepMap.cs`

**Como Verificar/Corrigir:**

1. **Verificar todas as entidades:**
```bash
# Buscar por uso de System.Guid em entidades
grep -r "virtual Guid " --include="*.cs" SW_PortalProprietario.Domain
```

2. **Se encontrar entidades com `Guid`, há 3 opções:**

**Opção A: Converter para `string` (Recomendado)**
```csharp
// ANTES
public virtual Guid ObjectGuid { get; set; }

// DEPOIS
public virtual string? ObjectGuid { get; set; }

// E no código, ao setar:
entity.ObjectGuid = Guid.NewGuid().ToString();
```

**Opção B: Criar mapeamento explícito**
```csharp
public class MinhaEntidadeMap : ClassMap<MinhaEntidade>
{
    public MinhaEntidadeMap()
    {
        Id(x => x.Id).GeneratedBy.Native("MinhaEntidade_");
        
        // Para GUID como string
        Map(x => x.ObjectGuid).Length(100).Nullable();
        
        // OU para GUID como UUID (PostgreSQL)
        // Map(x => x.ObjectGuid).CustomType("uuid").Length(36);
        
        Table("MinhaEntidade");
    }
}
```

**Opção C: Usar UUID no PostgreSQL (Avançado)**
```csharp
// No mapeamento:
Map(x => x.ObjectGuid).CustomSqlType("uuid");

// Requer que a propriedade seja:
public virtual Guid ObjectGuid { get; set; }
```

### Erro: "permission denied for schema portalohana"

**Solução:** O usuário não tem permissões no schema. Execute:

```sql
GRANT ALL PRIVILEGES ON SCHEMA portalohana TO seu_usuario;
```

### Erro: "schema 'portalohana' does not exist"

**Solução:** Crie o schema:

```sql
CREATE SCHEMA IF NOT EXISTS portalohana;
GRANT ALL PRIVILEGES ON SCHEMA portalohana TO seu_usuario;
```

### Erro: "relation already exists"

**Solução:** As tabelas já existem. O NHibernate está tentando recriar. Isso pode ser ignorado ou você pode usar `SchemaUpdate` ao invés de `SchemaExport`.

### Erro: "timeout expired"

**Solução:** Aumente o timeout da conexão:

```csharp
.Raw("hibernate.c3p0.timeout", "600")  // Aumentar para 10 minutos
```

## Modo de Depuração Avançado

Para obter mais informações de debug do NHibernate, adicione ao `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "NHibernate": "Debug",
      "NHibernate.SQL": "Debug"
    }
  }
}
```

## Exportar Schema para Arquivo (Para Debug)

Para gerar um script SQL com todas as tabelas que seriam criadas:

```csharp
// Adicione este código temporariamente no método BuildSchema:
var schemaExport = new SchemaExport(configuration);
schemaExport.SetOutputFile(@"C:\temp\schema.sql");
schemaExport.Create(scriptAction: null, execute: false);
```

Isso gerará um arquivo `schema.sql` que você pode revisar e executar manualmente.

## Logs de SQL Detalhados

Com as configurações atuais, você verá cada comando SQL sendo executado:

```
[SQL] CREATE TABLE portalohana.usuario (
    Id INT NOT NULL,
    Login VARCHAR(255),
    PasswordHash VARCHAR(255),
    PRIMARY KEY (Id)
)
```

## Próximos Passos

1. Execute a aplicação
2. Observe os logs no console
3. Se houver erros, copie a mensagem de erro completa
4. Verifique as permissões do banco de dados
5. Se necessário, execute os comandos SQL de permissões acima
6. Reinicie a aplicação

## Suporte

Se os problemas persistirem, forneça:
- Mensagem de erro completa do console
- String de conexão (sem senha)
- Versão do PostgreSQL
- Permissões atuais do usuário no banco de dados
