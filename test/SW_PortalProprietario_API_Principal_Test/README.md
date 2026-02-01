# Testes - SW Portal Proprietário API

Este diretório contém os testes unitários e de integração para a API do SW Portal Proprietário.

## Estrutura

```
test/SW_PortalProprietario_API_Principal_Test/
├── Base/
│   └── TestBase.cs              # Classe base para testes de integração
├── Controllers/
│   ├── AuthControllerTest.cs     # Testes do AuthController
│   └── CidadeControllerTest.cs   # Testes do CidadeController
├── Services/
│   └── CityServiceTest.cs        # Testes do CityService
├── CidadeTest/                   # Testes de mapeamento (existentes)
├── PaisTest/                     # Testes de mapeamento (existentes)
└── SW_PortalProprietario.Test.csproj
```

## Tecnologias Utilizadas

- **xUnit**: Framework de testes
- **Moq**: Biblioteca para criação de mocks
- **FluentAssertions**: Biblioteca para asserções mais legíveis
- **Microsoft.AspNetCore.Mvc.Testing**: Para testes de integração com WebApplicationFactory
- **Coverlet**: Para cobertura de código

## Como Executar os Testes

### Executar todos os testes

```bash
dotnet test
```

### Executar testes com cobertura

**Método 1: Usando o coletor padrão (recomendado)**
```bash
dotnet test SW_PortalProprietario.Test.csproj --collect:"XPlat Code Coverage" --results-directory:"./coverage"
```

**Método 2: Usando Coverlet diretamente**
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

**Método 3: Usando o script PowerShell**
```powershell
.\run-tests-with-coverage.ps1
```

### Visualizar percentual de cobertura

Após executar os testes com cobertura, você pode:

1. **Ver o resumo no terminal:**
```powershell
$coverageFile = Get-ChildItem -Path "coverage" -Filter "coverage.cobertura.xml" -Recurse | Select-Object -First 1
[xml]$xml = Get-Content $coverageFile.FullName
$summary = $xml.coverage
Write-Host "Linhas: $([math]::Round([double]$summary.'line-rate' * 100, 2))%"
Write-Host "Branches: $([math]::Round([double]$summary.'branch-rate' * 100, 2))%"
```

2. **Gerar relatório HTML (requer ReportGenerator):**
```bash
# Instalar ReportGenerator (uma vez)
dotnet tool install -g dotnet-reportgenerator-globaltool

# Gerar relatório
reportgenerator -reports:"coverage/**/coverage.cobertura.xml" -targetdir:"coverage/html" -reporttypes:"Html;Badges"
```

3. **Abrir o relatório HTML:**
```bash
# No Windows
start coverage/html/index.html
```

### Executar testes específicos

```bash
dotnet test --filter "FullyQualifiedName~AuthControllerTest"
```

### Executar testes no modo watch (desenvolvimento)

```bash
dotnet watch test
```

## Padrões de Teste

### Testes de Controllers

Os testes de controllers usam **Moq** para mockar os serviços e verificam:
- Status codes corretos
- Estrutura de resposta adequada
- Tratamento de erros

Exemplo:
```csharp
[Fact(DisplayName = "SaveCity - Deve retornar 200 OK quando cidade é salva com sucesso")]
public async Task SaveCity_DeveRetornar200Ok_QuandoCidadeSalvaComSucesso()
{
    // Arrange
    var inputModel = new RegistroCidadeInputModel { ... };
    _cityServiceMock.Setup(x => x.SaveCity(It.IsAny<RegistroCidadeInputModel>()))
                   .ReturnsAsync(cidadeModel);

    // Act
    var result = await _controller.SaveCity(inputModel);

    // Assert
    var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    // ...
}
```

### Testes de Services

Os testes de services mockam as dependências (repositórios, mappers, etc.) e verificam:
- Lógica de negócio
- Chamadas corretas aos repositórios
- Tratamento de exceções

Exemplo:
```csharp
[Fact(DisplayName = "DeleteCity - Deve deletar cidade com sucesso quando cidade existe")]
public async Task DeleteCity_DeveDeletarCidadeComSucesso_QuandoCidadeExiste()
{
    // Arrange
    _repositoryMock.Setup(x => x.FindById<Cidade>(id))
                   .ReturnsAsync(cidade);
    
    // Act
    var result = await _service.DeleteCity(id);
    
    // Assert
    result.Result.Should().Be("Removido com sucesso!");
    _repositoryMock.Verify(x => x.Remove(cidade), Times.Once);
}
```

## Convenções de Nomenclatura

- **Métodos de teste**: `NomeDoMetodo_DeveFazerAlgo_QuandoCondicao`
- **DisplayName**: Descrição clara do que o teste verifica
- **Arrange-Act-Assert**: Padrão AAA para organização dos testes

## Cobertura de Código

A cobertura de código é coletada usando **Coverlet**. Para visualizar:

1. Execute os testes com cobertura:
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

2. Gere relatório HTML (requer ReportGenerator):
```bash
reportgenerator -reports:coverage.opencover.xml -targetdir:coverage
```

## Adicionando Novos Testes

1. **Para Controllers**: Crie um arquivo em `Controllers/` seguindo o padrão `*ControllerTest.cs`
2. **Para Services**: Crie um arquivo em `Services/` seguindo o padrão `*ServiceTest.cs`
3. **Para Mapeamentos**: Crie um diretório em `*Test/` seguindo o padrão existente

## Notas Importantes

- Certifique-se de que não há processos rodando a API antes de executar os testes (Visual Studio, IIS Express, etc.)
- Os testes de integração podem requerer configuração adicional (banco de dados, serviços externos)
- Use mocks para dependências externas (repositórios, serviços, APIs externas)

