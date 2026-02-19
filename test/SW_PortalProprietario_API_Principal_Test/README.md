# Testes - SW Portal ProprietÃ¡rio API

Este diretÃ³rio contÃ©m os testes unitÃ¡rios e de integraÃ§Ã£o para a API do SW Portal ProprietÃ¡rio.

## Estrutura

```
test/SW_PortalProprietario_API_Principal_Test/
â”œâ”€â”€ Base/
â”‚   â””â”€â”€ TestBase.cs              # Classe base para testes de integraÃ§Ã£o
â”œâ”€â”€ Controllers/
â”‚   â”œâ”€â”€ AuthControllerTest.cs     # Testes do AuthController
â”‚   â””â”€â”€ CidadeControllerTest.cs   # Testes do CidadeController
â”œâ”€â”€ Services/
â”‚   â””â”€â”€ CityServiceTest.cs        # Testes do CityService
â”œâ”€â”€ CidadeTest/                   # Testes de mapeamento (existentes)
â”œâ”€â”€ PaisTest/                     # Testes de mapeamento (existentes)
â””â”€â”€ SW_PortalProprietario.Test.csproj
```

## Tecnologias Utilizadas

- **xUnit**: Framework de testes
- **Moq**: Biblioteca para criaÃ§Ã£o de mocks
- **FluentAssertions**: Biblioteca para asserÃ§Ãµes mais legÃ­veis
- **Microsoft.AspNetCore.Mvc.Testing**: Para testes de integraÃ§Ã£o com WebApplicationFactory
- **Coverlet**: Para cobertura de cÃ³digo

## Como Executar os Testes

### Executar todos os testes

```bash
dotnet test
```

### Executar testes com cobertura

**MÃ©todo 1: Usando o coletor padrÃ£o (recomendado)**
```bash
dotnet test SW_PortalProprietario.Test.csproj --collect:"XPlat Code Coverage" --results-directory:"./coverage"
```

**MÃ©todo 2: Usando Coverlet diretamente**
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

**MÃ©todo 3: Usando o script PowerShell**
```powershell
.\run-tests-with-coverage.ps1
```

### Visualizar percentual de cobertura

ApÃ³s executar os testes com cobertura, vocÃª pode:

1. **Ver o resumo no terminal:**
```powershell
$coverageFile = Get-ChildItem -Path "coverage" -Filter "coverage.cobertura.xml" -Recurse | Select-Object -First 1
[xml]$xml = Get-Content $coverageFile.FullName
$summary = $xml.coverage
Write-Host "Linhas: $([math]::Round([double]$summary.'line-rate' * 100, 2))%"
Write-Host "Branches: $([math]::Round([double]$summary.'branch-rate' * 100, 2))%"
```

2. **Gerar relatÃ³rio HTML (requer ReportGenerator):**
```bash
# Instalar ReportGenerator (uma vez)
dotnet tool install -g dotnet-reportgenerator-globaltool

# Gerar relatÃ³rio
reportgenerator -reports:"coverage/**/coverage.cobertura.xml" -targetdir:"coverage/html" -reporttypes:"Html;Badges"
```

3. **Abrir o relatÃ³rio HTML:**
```bash
# No Windows
start coverage/html/index.html
```

### Executar testes especÃ­ficos

```bash
dotnet test --filter "FullyQualifiedName~AuthControllerTest"
```

### Executar testes no modo watch (desenvolvimento)

```bash
dotnet watch test
```

## PadrÃµes de Teste

### Testes de Controllers

Os testes de controllers usam **Moq** para mockar os serviÃ§os e verificam:
- Status codes corretos
- Estrutura de resposta adequada
- Tratamento de erros

Exemplo:
```csharp
[Fact(DisplayName = "SaveCity - Deve retornar 200 OK quando cidade Ã© salva com sucesso")]
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

Os testes de services mockam as dependÃªncias (repositÃ³rios, mappers, etc.) e verificam:
- LÃ³gica de negÃ³cio
- Chamadas corretas aos repositÃ³rios
- Tratamento de exceÃ§Ãµes

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

## ConvenÃ§Ãµes de Nomenclatura

- **MÃ©todos de teste**: `NomeDoMetodo_DeveFazerAlgo_QuandoCondicao`
- **DisplayName**: DescriÃ§Ã£o clara do que o teste verifica
- **Arrange-Act-Assert**: PadrÃ£o AAA para organizaÃ§Ã£o dos testes

## Cobertura de CÃ³digo

A cobertura de cÃ³digo Ã© coletada usando **Coverlet**. Para visualizar:

1. Execute os testes com cobertura:
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

2. Gere relatÃ³rio HTML (requer ReportGenerator):
```bash
reportgenerator -reports:coverage.opencover.xml -targetdir:coverage
```

## Adicionando Novos Testes

1. **Para Controllers**: Crie um arquivo em `Controllers/` seguindo o padrÃ£o `*ControllerTest.cs`
2. **Para Services**: Crie um arquivo em `Services/` seguindo o padrÃ£o `*ServiceTest.cs`
3. **Para Mapeamentos**: Crie um diretÃ³rio em `*Test/` seguindo o padrÃ£o existente

## Notas Importantes

- Certifique-se de que nÃ£o hÃ¡ processos rodando a API antes de executar os testes (Visual Studio, IIS Express, etc.)
- Os testes de integraÃ§Ã£o podem requerer configuraÃ§Ã£o adicional (banco de dados, serviÃ§os externos)
- Use mocks para dependÃªncias externas (repositÃ³rios, serviÃ§os, APIs externas)

