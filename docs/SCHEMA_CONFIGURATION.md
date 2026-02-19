# Configuração do Schema PostgreSQL

## Visão Geral

Este documento descreve como o schema "portalohana" foi configurado no NHibernate para o PostgreSQL.

## Alterações Realizadas

Foram adicionadas configurações de `default_schema` em todos os arquivos de extensão do NHibernate que suportam PostgreSQL:

### 1. NHibernateExtensions.cs
Configuração para a conexão principal (`DEFAULT_CONNECTION`):
```csharp
.Raw("default_schema", "portalohana")
```

### 2. NHibernateExtensionsCM.cs
Configuração para a conexão CM (`CM_CONNECTION`):
```csharp
.Raw("default_schema", "portalohana")
```

### 3. NHibernateExtensionsPortalEsol.cs
Configuração para a conexão Portal eSolution (`ESOL_PORTAL_CONNECTION`):
```csharp
.Raw("default_schema", "portalohana")
```

### 4. NHibernateExtensionsAccessCenter.cs
Configuração para a conexão Access Center (`ESOL_ACCESS_CENTER_CONNECTION`):
```csharp
.Raw("default_schema", "portalohana")
```

### 5. NHibernateHostedServiceExtensions.cs
Configuração para os serviços hospedados (Hosted Services):
```csharp
.Raw("default_schema", "portalohana")
```

## Como Funciona

Quando você configura o `default_schema` no NHibernate:

1. **Todas as consultas SQL** geradas pelo NHibernate usarão automaticamente o schema configurado
2. **Não é necessário** adicionar `Schema()` em cada mapeamento individual
3. **Todas as tabelas** serão acessadas dentro do schema "portalohana"

Exemplo de query gerada:
```sql
-- Sem default_schema
SELECT * FROM Usuario WHERE Id = 1

-- Com default_schema = "portalohana"
SELECT * FROM portalohana.Usuario WHERE Id = 1
```

## Connection String

A connection string no arquivo `.env` deve estar configurada assim:

```env
DEFAULT_CONNECTION=Host=dbpg-moreia.bpark.net.br;Port=5440;Database=bdbp;Username=sw;Password=svtQY7q10xVi
```

**Nota:** O schema não precisa ser especificado na connection string, pois é configurado no NHibernate.

## Verificação

Para verificar se o schema está sendo usado corretamente:

1. **Habilitar logs SQL do NHibernate** (já está com `.ShowSql()` em alguns ambientes)
2. **Verificar os logs** e confirmar que as queries estão usando `portalohana.tabela`
3. **Testar consultas** e verificar se os dados estão sendo retornados corretamente

## Mapeamentos

Os mapeamentos FluentNHibernate continuam da mesma forma:

```csharp
public class UsuarioMap : ClassMap<Usuario>
{
    public UsuarioMap()
    {
        Id(x => x.Id).GeneratedBy.Sequence("USUARIO_SEQUENCE");
        
        Map(x => x.Nome);
        Map(x => x.Email);
        
        Table("Usuario"); // Não é necessário adicionar Schema("portalohana")
    }
}
```

O NHibernate automaticamente usará `portalohana.Usuario` por causa da configuração `default_schema`.

## Ambientes Suportados

Esta configuração funciona **apenas para PostgreSQL**. Para outros bancos de dados (SQL Server, Oracle, MySQL), o comportamento continua o mesmo de antes:

- **SQL Server**: Usa o schema padrão `dbo`
- **Oracle**: Usa o schema do usuário conectado
- **SQLite**: Não suporta schemas

## Migração de Dados

Se você está migrando de um schema diferente para "portalohana":

1. **Criar o schema** no PostgreSQL:
   ```sql
   CREATE SCHEMA portalohana;
   ```

2. **Mover as tabelas**:
   ```sql
   ALTER TABLE public.Usuario SET SCHEMA portalohana;
   ALTER TABLE public.Empresa SET SCHEMA portalohana;
   -- Repetir para todas as tabelas
   ```

3. **Ou migrar os dados**:
   ```sql
   CREATE TABLE portalohana.Usuario AS SELECT * FROM public.Usuario;
   ```

## Troubleshooting

### Erro: "relation does not exist"
- Verifique se o schema "portalohana" existe no banco
- Verifique se o usuário tem permissões no schema
- Confirme que as tabelas estão no schema correto

### Erro: "schema does not exist"
```sql
CREATE SCHEMA portalohana;
GRANT ALL ON SCHEMA portalohana TO sw;
```

### Permissões
```sql
-- Conceder permissões ao usuário
GRANT USAGE ON SCHEMA portalohana TO sw;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA portalohana TO sw;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA portalohana TO sw;
```

## Referências

- [NHibernate Configuration](https://nhibernate.info/doc/nhibernate-reference/session-configuration.html)
- [FluentNHibernate Wiki](https://github.com/FluentNHibernate/fluent-nhibernate/wiki)
- [PostgreSQL Schemas](https://www.postgresql.org/docs/current/ddl-schemas.html)

## Suporte

Para dúvidas ou problemas, contate a equipe de desenvolvimento.

---

**Última atualização:** $(Get-Date -Format "yyyy-MM-dd")
**Responsável:** Equipe de Desenvolvimento SW Soluções
