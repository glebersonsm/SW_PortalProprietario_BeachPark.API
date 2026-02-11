# ?? Guia de Contribuição

Obrigado por considerar contribuir com o projeto SW Portal Proprietário!

## ?? Índice

- [Como Contribuir](#-como-contribuir)
- [Padrões de Código](#-padrões-de-código)
- [Processo de Pull Request](#-processo-de-pull-request)
- [Reportar Bugs](#-reportar-bugs)
- [Sugerir Melhorias](#-sugerir-melhorias)

---

## ?? Como Contribuir

### 1. Fork o Repositório

```bash
# Fazer fork no GitHub e clonar localmente
git clone https://github.com/seu-usuario/SW_PortalProprietario_BeachPark.API.git
cd SW_PortalProprietario_BeachPark.API
```

### 2. Criar uma Branch

```bash
# Criar branch a partir da master
git checkout -b feature/minha-feature
# ou
git checkout -b fix/meu-bug-fix
```

### 3. Fazer Alterações

- Escreva código limpo e bem documentado
- Siga os padrões de código do projeto
- Adicione testes quando aplicável
- Atualize a documentação se necessário

### 4. Commit

```bash
# Adicionar arquivos alterados
git add .

# Commit com mensagem descritiva
git commit -m "feat: adiciona nova funcionalidade X"
# ou
git commit -m "fix: corrige bug Y"
```

### 5. Push e Pull Request

```bash
# Enviar para seu fork
git push origin feature/minha-feature

# Criar Pull Request no GitHub
```

---

## ?? Padrões de Código

### Convenções de Nomenclatura

#### Classes e Interfaces
```csharp
// PascalCase para classes
public class UserService { }

// PascalCase com prefixo I para interfaces
public interface IUserService { }
```

#### Métodos
```csharp
// PascalCase para métodos públicos
public async Task<User> GetUserById(int id) { }

// camelCase para métodos privados
private async Task validateUser(User user) { }
```

#### Variáveis
```csharp
// camelCase para variáveis locais
var userName = "João";

// _camelCase para campos privados
private readonly ILogger _logger;

// PascalCase para propriedades
public string UserName { get; set; }
```

### Padrões de Código

#### Async/Await
```csharp
// ? Correto
public async Task<User> GetUserAsync(int id)
{
    var user = await _repository.FindByIdAsync(id);
    return user;
}

// ? Incorreto
public User GetUser(int id)
{
    var user = _repository.FindByIdAsync(id).Result;
    return user;
}
```

#### Tratamento de Exceções
```csharp
// ? Correto
try
{
    _repository.BeginTransaction();
    await _repository.Save(entity);
    await _repository.CommitAsync();
}
catch (Exception ex)
{
    _repository.Rollback();
    _logger.LogError(ex, "Erro ao salvar entidade");
    throw;
}

// ? Incorreto - nunca engolir exceções
catch (Exception ex)
{
    // Fazer nada
}
```

#### Injeção de Dependência
```csharp
// ? Correto - injetar interfaces
public class UserService : IUserService
{
    private readonly IRepository _repository;
    private readonly ILogger<UserService> _logger;
    
    public UserService(IRepository repository, ILogger<UserService> logger)
    {
        _repository = repository;
        _logger = logger;
    }
}

// ? Incorreto - instanciar diretamente
public class UserService
{
    private readonly Repository _repository = new Repository();
}
```

### Estrutura de Arquivos

```
SW_PortalProprietario.Application/
??? Services/
?   ??? Core/
?   ?   ??? Interfaces/
?   ?   ?   ??? IUserService.cs
?   ?   ??? UserService.cs
?   ??? Providers/
??? Models/
?   ??? AuthModels/
?   ??? SystemModels/
??? Functions/
```

---

## ?? Processo de Pull Request

### Checklist do PR

Antes de submeter seu PR, verifique:

- [ ] O código compila sem erros
- [ ] Todos os testes passam
- [ ] Novos testes foram adicionados (se aplicável)
- [ ] A documentação foi atualizada (se aplicável)
- [ ] O código segue os padrões do projeto
- [ ] Commit messages são descritivas
- [ ] Não há conflitos com a branch master

### Template de PR

```markdown
## Descrição
Breve descrição das alterações

## Tipo de Mudança
- [ ] Bug fix
- [ ] Nova feature
- [ ] Breaking change
- [ ] Documentação

## Como Testar
Passos para testar as alterações

## Checklist
- [ ] Código compila
- [ ] Testes passam
- [ ] Documentação atualizada
```

### Convenções de Commit

Seguimos o padrão [Conventional Commits](https://www.conventionalcommits.org/):

```
<tipo>(<escopo>): <descrição>

[corpo opcional]

[rodapé opcional]
```

**Tipos:**
- `feat`: Nova funcionalidade
- `fix`: Correção de bug
- `docs`: Alteração de documentação
- `style`: Formatação, ponto e vírgula faltando, etc
- `refactor`: Refatoração de código
- `test`: Adição ou modificação de testes
- `chore`: Manutenção, configuração, etc

**Exemplos:**
```
feat(auth): adiciona autenticação 2FA
fix(user): corrige validação de email
docs(deploy): atualiza guia de deploy Linux
refactor(repository): melhora performance de consultas
```

---

## ?? Reportar Bugs

### Antes de Reportar

1. Verifique se o bug já foi reportado nas [Issues](https://github.com/glebersonsm/SW_PortalProprietario_BeachPark.API/issues)
2. Certifique-se de que está usando a versão mais recente
3. Tente reproduzir o bug em ambiente limpo

### Template de Bug Report

```markdown
## Descrição do Bug
Descrição clara e concisa do bug

## Passos para Reproduzir
1. Ir para '...'
2. Clicar em '....'
3. Ver erro

## Comportamento Esperado
O que deveria acontecer

## Comportamento Atual
O que está acontecendo

## Screenshots
Se aplicável

## Ambiente
- OS: [ex: Ubuntu 22.04]
- .NET Version: [ex: 8.0.1]
- Navegador: [ex: Chrome 120]

## Logs
```
Cole logs relevantes aqui
```

## Informações Adicionais
Qualquer outra informação relevante
```

---

## ?? Sugerir Melhorias

### Template de Feature Request

```markdown
## Problema
Descreva o problema que esta feature resolveria

## Solução Proposta
Descrição clara da solução proposta

## Alternativas Consideradas
Outras soluções que você considerou

## Informações Adicionais
Mockups, exemplos de código, etc
```

---

## ?? Testes

### Executar Testes

```bash
# Executar todos os testes
dotnet test

# Executar testes específicos
dotnet test --filter "FullyQualifiedName~UserServiceTest"

# Executar com cobertura
dotnet test /p:CollectCoverage=true
```

### Escrever Testes

```csharp
[Fact(DisplayName = "GetUser - Deve retornar usuário quando existe")]
public async Task GetUser_DeveRetornarUsuario_QuandoExiste()
{
    // Arrange
    var userId = 1;
    var expectedUser = new User { Id = userId, Name = "Teste" };
    _repositoryMock.Setup(x => x.GetByIdAsync(userId))
                   .ReturnsAsync(expectedUser);

    // Act
    var result = await _service.GetUserAsync(userId);

    // Assert
    result.Should().NotBeNull();
    result.Should().BeEquivalentTo(expectedUser);
}
```

---

## ?? Documentação

### Atualizar Documentação

Ao fazer alterações, atualize:

1. **README.md** - Se a mudança afeta o uso geral
2. **docs/DEPLOY_LINUX.md** - Se afeta o deploy
3. **XML Comments** - Para métodos públicos
4. **CHANGELOG.md** - Adicionar entrada

### Exemplo de Comentário XML

```csharp
/// <summary>
/// Obtém um usuário pelo ID.
/// </summary>
/// <param name="id">ID do usuário</param>
/// <returns>Usuário encontrado ou null</returns>
/// <exception cref="ArgumentException">Se o ID for inválido</exception>
public async Task<User?> GetUserAsync(int id)
{
    if (id <= 0)
        throw new ArgumentException("ID deve ser maior que zero", nameof(id));
    
    return await _repository.GetByIdAsync(id);
}
```

---

## ?? Boas Práticas

### DOs

? Escreva código limpo e legível  
? Adicione comentários explicativos quando necessário  
? Siga os padrões do projeto  
? Escreva testes para novas funcionalidades  
? Mantenha os commits pequenos e focados  
? Atualize a documentação  
? Peça ajuda quando necessário  

### DON'Ts

? Commitar código que não compila  
? Fazer commit de arquivos de configuração com senhas  
? Fazer PRs gigantes com muitas mudanças  
? Ignorar warnings do compilador  
? Deixar código comentado  
? Usar valores hardcoded  
? Fazer merge direto na master  

---

## ?? Suporte

Se tiver dúvidas sobre como contribuir:

- **Email:** contato@swsolucoes.inf.br
- **Issues:** https://github.com/glebersonsm/SW_PortalProprietario_BeachPark.API/issues
- **Discussions:** https://github.com/glebersonsm/SW_PortalProprietario_BeachPark.API/discussions

---

## ?? Licença

Ao contribuir, você concorda que suas contribuições serão licenciadas sob a mesma licença do projeto.

---

**Obrigado por contribuir! ??**

---

Última atualização: Janeiro 2024
