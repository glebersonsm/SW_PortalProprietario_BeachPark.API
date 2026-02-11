# Schema Auto-Prefix para PostgreSQL

## ?? Visão Geral

O sistema agora adiciona **automaticamente** o schema name antes de cada tabela em queries SQL quando usando PostgreSQL, eliminando a necessidade de escrever manualmente `schemaname.tabela` em cada query.

## ?? Funcionalidade

### Métodos Afetados

Os seguintes métodos do `RepositoryHosted` agora aplicam automaticamente o schema:

1. ? **`FindBySql<T>`** - Consultas SQL diretas
2. ? **`CountTotalEntry`** - Contagem de registros
3. ? **`GetParametroSistemaViewModel`** - Consulta de parâmetros do sistema

### Como Funciona

#### Antes (Manual)
```csharp
var sql = "SELECT * FROM portalohana.Usuario WHERE Id = 1";
var usuarios = await repository.FindBySql<Usuario>(sql);
```

#### Agora (Automático)
```csharp
// Escreve sem schema - o sistema adiciona automaticamente!
var sql = "SELECT * FROM Usuario WHERE Id = 1";
var usuarios = await repository.FindBySql<Usuario>(sql);

// O SQL executado será: SELECT * FROM portalohana.Usuario WHERE Id = 1
```

## ?? Configuração

### Método 1: Arquivo de Configuração (Recomendado)

Adicione ao `appsettings.json` ou `BeachParkConfigurations.json`:

```json
{
  "PostgreSQL": {
    "DefaultSchema": "portalohana"
  }
}
```

### Método 2: Connection String

Adicione à connection string:

```
Host=localhost;PORT=5432;Database=mydb;Username=user;Password=pass;SearchPath=portalohana
```

### Método 3: Padrão Automático

Se não houver configuração, o sistema usa `portalohana` como padrão.

## ?? Exemplos de Uso

### Exemplo 1: Query Simples
```csharp
// ANTES
var sql = "SELECT * FROM portalohana.Usuario WHERE Login = :login";

// AGORA
var sql = "SELECT * FROM Usuario WHERE Login = :login";
var usuarios = await repository.FindBySql<Usuario>(sql, new Parameter("login", "admin"));
```

### Exemplo 2: Query com JOIN
```csharp
// ANTES
var sql = @"SELECT u.*, p.Nome 
            FROM portalohana.Usuario u 
            INNER JOIN portalohana.Pessoa p ON u.Pessoa = p.Id";

// AGORA
var sql = @"SELECT u.*, p.Nome 
            FROM Usuario u 
            INNER JOIN Pessoa p ON u.Pessoa = p.Id";
var dados = await repository.FindBySql<UsuarioViewModel>(sql);
```

### Exemplo 3: Query Complexa
```csharp
// ANTES
var sql = @"SELECT e.*, COUNT(u.Id) as TotalUsuarios
            FROM portalohana.Empresa e
            LEFT JOIN portalohana.Usuario u ON u.Empresa = e.Id
            WHERE e.Ativo = true
            GROUP BY e.Id";

// AGORA
var sql = @"SELECT e.*, COUNT(u.Id) as TotalUsuarios
            FROM Empresa e
            LEFT JOIN Usuario u ON u.Empresa = e.Id
            WHERE e.Ativo = true
            GROUP BY e.Id";
var empresas = await repository.FindBySql<EmpresaViewModel>(sql);
```

### Exemplo 4: CountTotalEntry
```csharp
// ANTES
var sql = "SELECT * FROM portalohana.AuditLog WHERE UserId = :userId";

// AGORA
var sql = "SELECT * FROM AuditLog WHERE UserId = :userId";
var total = await repository.CountTotalEntry(sql, null, new Parameter("userId", 123));
```

## ??? Compatibilidade

### ? O que funciona automaticamente:

- ? `FROM Usuario`
- ? `JOIN Pessoa`
- ? `INNER JOIN Empresa`
- ? `LEFT JOIN AuditLog`
- ? `UPDATE Usuario`
- ? `INTO Empresa`

### ?? O que NÃO é modificado:

- ? Tabelas que **já têm schema**: `portalohana.Usuario` (mantém como está)
- ? Subqueries com `SELECT`
- ? Palavras reservadas como `DUAL`, `VALUES`
- ? Bancos de dados diferentes de PostgreSQL (SQL Server, Oracle, etc.)

## ?? Detalhes Técnicos

### Algoritmo

1. **Detecta o tipo de banco**: Só aplica para PostgreSQL
2. **Obtém o schema**: Da configuração ou connection string
3. **Identifica tabelas**: Usando regex para encontrar padrões `FROM tabela`, `JOIN tabela`, etc.
4. **Adiciona schema**: Somente se a tabela ainda não tiver schema
5. **Preserva aliases**: Mantém aliases e subqueries intactos

### Pattern Regex

```regex
\b(FROM|JOIN|INTO|UPDATE|TABLE)\s+(?!(?:[a-zA-Z_][a-zA-Z0-9_]*\.))([a-zA-Z_][a-zA-Z0-9_]*)
```

- Captura palavras-chave SQL que precedem tabelas
- Ignora tabelas que já têm schema (formato `schema.tabela`)
- Captura apenas identificadores válidos

## ?? Benefícios

### ? Vantagens

1. **Menos código repetitivo**: Não precisa escrever `portalohana.` toda hora
2. **Mais legível**: Queries ficam mais limpas
3. **Manutenível**: Se mudar o schema, só altera em um lugar
4. **Retrocompatível**: Queries com schema explícito continuam funcionando
5. **Seguro**: Só aplica para PostgreSQL, não afeta outros bancos

### ? Performance

- **Impacto mínimo**: Processamento de string leve
- **Execução única**: Aplicado uma vez por query
- **Cache-friendly**: Resultado pode ser cacheado se necessário

## ?? Casos Especiais

### Caso 1: Schema Explícito (Preservado)

```csharp
// Se você REALMENTE quer especificar um schema diferente:
var sql = "SELECT * FROM outro_schema.Tabela";
var dados = await repository.FindBySql<Tabela>(sql);
// Resultado: SELECT * FROM outro_schema.Tabela (inalterado)
```

### Caso 2: Subqueries

```csharp
var sql = @"SELECT * FROM (
                SELECT Id, Nome FROM Usuario
            ) subquery";
// Resultado: SELECT * FROM (
//                SELECT Id, Nome FROM portalohana.Usuario
//            ) subquery
```

### Caso 3: SQL Server / Oracle

```csharp
// Quando conectado ao SQL Server ou Oracle, nada é modificado
var sql = "SELECT * FROM Usuario";
// Resultado: SELECT * FROM Usuario (inalterado)
```

## ?? Troubleshooting

### Problema: Schema não está sendo aplicado

**Verificações:**
1. Certifique-se de que está usando PostgreSQL
2. Verifique a configuração do schema
3. Confirme que está usando `FindBySql` (não HQL)

```csharp
// Debug: Verificar tipo de banco
var dbType = repository.DataBaseType;
Console.WriteLine($"Database Type: {dbType}"); // Deve ser PostgreSql
```

### Problema: Query quebrou após adicionar schema

**Possíveis causas:**
- Tabela não existe no schema especificado
- Nome da tabela está incorreto
- Tabela está em outro schema

**Solução:** Use schema explícito:
```csharp
var sql = "SELECT * FROM public.MinhaTabela"; // Força uso do schema 'public'
```

## ?? Migração de Código Existente

### Passo 1: Identificar Queries

Busque por queries SQL no código:
```bash
# Buscar queries com schema hardcoded
grep -r "portalohana\." --include="*.cs"
```

### Passo 2: Remover Schema Manual

```diff
- var sql = "SELECT * FROM portalohana.Usuario WHERE Id = :id";
+ var sql = "SELECT * FROM Usuario WHERE Id = :id";
```

### Passo 3: Testar

Execute os testes para garantir que as queries continuam funcionando.

## ?? Boas Práticas

### ? Faça

```csharp
// Escreva SQL limpo sem schema
var sql = "SELECT * FROM Usuario WHERE Login = :login";

// Use aliases descritivos
var sql = "SELECT u.*, p.Nome FROM Usuario u JOIN Pessoa p ON u.Pessoa = p.Id";

// Deixe o sistema gerenciar o schema
var usuarios = await repository.FindBySql<Usuario>(sql);
```

### ? Não Faça

```csharp
// Não hardcode o schema (a menos que seja necessário)
var sql = "SELECT * FROM portalohana.Usuario"; // Evite!

// Não misture schemas sem motivo
var sql = "SELECT * FROM portalohana.Usuario u JOIN public.Pessoa p"; // Confuso!
```

## ?? Referências

- **Arquivo**: `SW_PortalProprietario.Infra.Data\Repositories\Core\RepositoryHosted.cs`
- **Métodos**:
  - `AddSchemaToTables(string sql)`: Aplica schema automaticamente
  - `GetSchemaName()`: Obtém schema da configuração
  - `FindBySql<T>()`: Usa auto-prefix
  - `CountTotalEntry()`: Usa auto-prefix

## ?? Changelog

- **v1.0**: Implementação inicial
  - Auto-prefix para `FROM`, `JOIN`, `INTO`, `UPDATE`, `TABLE`
  - Suporte a configuração via `appsettings.json`
  - Fallback para `portalohana` como padrão
  - Compatível apenas com PostgreSQL
