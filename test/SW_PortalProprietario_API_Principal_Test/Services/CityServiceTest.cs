using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Services.Core;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Domain.Entities.Core.Geral;
using SW_PortalProprietario.Domain.Functions;
using Xunit;

namespace SW_PortalProprietario.Test.Services
{
    public class CityServiceTest
    {
        private readonly Mock<IRepositoryNH> _repositoryMock;
        private readonly Mock<ILogger<CityService>> _loggerMock;
        private readonly Mock<IServiceBase> _serviceBaseMock;
        private readonly Mock<IProjectObjectMapper> _mapperMock;
        private readonly Mock<ICommunicationProvider> _communicationProviderMock;
        private readonly CityService _service;

        public CityServiceTest()
        {
            _repositoryMock = new Mock<IRepositoryNH>();
            _loggerMock = new Mock<ILogger<CityService>>();
            _serviceBaseMock = new Mock<IServiceBase>();
            _mapperMock = new Mock<IProjectObjectMapper>();
            _communicationProviderMock = new Mock<ICommunicationProvider>();

            _service = new CityService(
                _repositoryMock.Object,
                _loggerMock.Object,
                _serviceBaseMock.Object,
                _mapperMock.Object,
                _communicationProviderMock.Object
            );
        }

        [Fact(DisplayName = "DeleteCity - Deve deletar cidade com sucesso quando cidade existe")]
        public async Task DeleteCity_DeveDeletarCidadeComSucesso_QuandoCidadeExiste()
        {
            // Arrange
            var id = 1;
            var cidade = new Cidade
            {
                Id = id,
                Nome = "SÃ£o Paulo",
                CodigoIbge = "3550308"
            };

            _repositoryMock
                .Setup(x => x.FindById<Cidade>(id))
                .ReturnsAsync(cidade);

            _repositoryMock
                .Setup(x => x.CommitAsync())
                .ReturnsAsync((true, (Exception?)null));

            // Act
            var result = await _service.DeleteCity(id);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(id);
            result.Result.Should().Be("Removido com sucesso!");

            _repositoryMock.Verify(x => x.BeginTransaction(), Times.Once);
            _repositoryMock.Verify(x => x.Remove(cidade), Times.Once);
            _repositoryMock.Verify(x => x.CommitAsync(), Times.Once);
            _serviceBaseMock.Verify(x => x.Compare(cidade, null), Times.Once);
        }

        [Fact(DisplayName = "DeleteCity - Deve lanÃ§ar ArgumentException quando cidade nÃ£o existe")]
        public async Task DeleteCity_DeveLancarArgumentException_QuandoCidadeNaoExiste()
        {
            // Arrange
            var id = 999;

            _repositoryMock
                .Setup(x => x.FindById<Cidade>(id))
                .ReturnsAsync((Cidade?)null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () => await _service.DeleteCity(id));

            _repositoryMock.Verify(x => x.Rollback(), Times.Once);
        }

        [Fact(DisplayName = "SaveCity - Deve salvar nova cidade com sucesso")]
        public async Task SaveCity_DeveSalvarNovaCidadeComSucesso()
        {
            // Arrange
            var inputModel = new RegistroCidadeInputModel
            {
                Nome = "SÃ£o Paulo",
                CodigoIbge = "3550308",
                EstadoId = 1
            };

            var cidadeEntity = new Cidade
            {
                Id = 1,
                Nome = "SÃ£o Paulo",
                CodigoIbge = "3550308"
            };

            var cidadeModel = new CidadeModel
            {
                Id = 1,
                Nome = "SÃ£o Paulo",
                CodigoIbge = "3550308"
            };

            _repositoryMock
                .Setup(x => x.FindByHql<Cidade>(It.IsAny<string>()))
                .ReturnsAsync(new List<Cidade>());

            _mapperMock
                .Setup(x => x.Map(It.IsAny<RegistroCidadeInputModel>(), It.IsAny<Cidade>()))
                .Returns(cidadeEntity);

            _repositoryMock
                .Setup(x => x.Save(It.IsAny<Cidade>()))
                .ReturnsAsync(cidadeEntity);

            _repositoryMock
                .Setup(x => x.CommitAsync())
                .ReturnsAsync((true, (Exception?)null));

            _mapperMock
                .Setup(x => x.Map(It.IsAny<Cidade>(), It.IsAny<CidadeModel>()))
                .Returns(cidadeModel);

            // Act
            var result = await _service.SaveCity(inputModel);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.Nome.Should().Be("SÃ£o Paulo");
            result.CodigoIbge.Should().Be("3550308");

            _repositoryMock.Verify(x => x.BeginTransaction(), Times.Once);
            _repositoryMock.Verify(x => x.Save(It.IsAny<Cidade>()), Times.Once);
            _repositoryMock.Verify(x => x.CommitAsync(), Times.Once);
        }

        [Fact(DisplayName = "UpdateCity - Deve atualizar cidade existente com sucesso")]
        public async Task UpdateCity_DeveAtualizarCidadeExistenteComSucesso()
        {
            // Arrange
            var inputModel = new AlteracaoCidadeInputModel
            {
                Id = 1,
                Nome = "SÃ£o Paulo Atualizado",
                CodigoIbge = "3550308",
                EstadoId = 1
            };

            var cidadeExistente = new Cidade
            {
                Id = 1,
                Nome = "SÃ£o Paulo",
                CodigoIbge = "3550308"
            };

            var estado = new Estado
            {
                Id = 1,
                Nome = "SÃ£o Paulo",
                Sigla = "SP"
            };

            var cidadeModel = new CidadeModel
            {
                Id = 1,
                Nome = "SÃ£o Paulo Atualizado",
                CodigoIbge = "3550308"
            };

            _repositoryMock
                .Setup(x => x.FindByHql<Cidade>(It.IsAny<string>()))
                .ReturnsAsync(new List<Cidade> { cidadeExistente });

            _repositoryMock
                .Setup(x => x.FindByHql<Estado>(It.IsAny<string>()))
                .ReturnsAsync(new List<Estado> { estado });

            _serviceBaseMock
                .Setup(x => x.GetObjectOld<Cidade>(It.IsAny<int>()))
                .ReturnsAsync(cidadeExistente);

            _mapperMock
                .Setup(x => x.Map(It.IsAny<AlteracaoCidadeInputModel>(), It.IsAny<Cidade>()))
                .Returns(cidadeExistente);

            _repositoryMock
                .Setup(x => x.Save(It.IsAny<Cidade>()))
                .ReturnsAsync(cidadeExistente);

            _repositoryMock
                .Setup(x => x.CommitAsync())
                .ReturnsAsync((true, (Exception?)null));

            _repositoryMock
                .Setup(x => x.FindBySql<CidadeModel>(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<SW_Utils.Auxiliar.Parameter[]>()))
                .ReturnsAsync(new List<CidadeModel> { cidadeModel });

            // Act
            var result = await _service.UpdateCity(inputModel);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.Nome.Should().Be("SÃ£o Paulo Atualizado");

            _repositoryMock.Verify(x => x.BeginTransaction(), Times.Once);
            _repositoryMock.Verify(x => x.Save(It.IsAny<Cidade>()), Times.Once);
            _repositoryMock.Verify(x => x.CommitAsync(), Times.Once);
        }

        [Fact(DisplayName = "UpdateCity - Deve lanÃ§ar ArgumentException quando cidade nÃ£o existe")]
        public async Task UpdateCity_DeveLancarArgumentException_QuandoCidadeNaoExiste()
        {
            // Arrange
            var inputModel = new AlteracaoCidadeInputModel
            {
                Id = 999,
                Nome = "Cidade Inexistente",
                CodigoIbge = "9999999"
            };

            _repositoryMock
                .Setup(x => x.FindByHql<Cidade>(It.IsAny<string>()))
                .ReturnsAsync(new List<Cidade>());

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () => await _service.UpdateCity(inputModel));

            _repositoryMock.Verify(x => x.Rollback(), Times.Once);
        }
    }
}

