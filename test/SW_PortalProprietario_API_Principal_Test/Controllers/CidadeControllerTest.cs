using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SW_PortalCliente_BeachPark.API.src.Controllers;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using Xunit;

namespace SW_PortalProprietario.Test.Controllers
{
    public class CidadeControllerTest
    {
        private readonly Mock<ICityService> _cityServiceMock;
        private readonly Mock<ILogger<CidadeController>> _loggerMock;
        private readonly CidadeController _controller;

        public CidadeControllerTest()
        {
            _cityServiceMock = new Mock<ICityService>();
            _loggerMock = new Mock<ILogger<CidadeController>>();
            _controller = new CidadeController(_cityServiceMock.Object);
        }

        [Fact(DisplayName = "SaveCity - Deve retornar 200 OK quando cidade Ã© salva com sucesso")]
        public async Task SaveCity_DeveRetornar200Ok_QuandoCidadeSalvaComSucesso()
        {
            // Arrange
            var inputModel = new RegistroCidadeInputModel
            {
                Nome = "SÃ£o Paulo",
                CodigoIbge = "3550308",
                EstadoId = 1
            };

            var cidadeModel = new CidadeModel
            {
                Id = 1,
                Nome = "SÃ£o Paulo",
                CodigoIbge = "3550308"
            };

            _cityServiceMock
                .Setup(x => x.SaveCity(It.IsAny<RegistroCidadeInputModel>()))
                .ReturnsAsync(cidadeModel);

            // Act
            var result = await _controller.SaveCity(inputModel);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var resultModel = okResult.Value.Should().BeOfType<ResultModel<CidadeModel>>().Subject;
            
            resultModel.Success.Should().BeTrue();
            resultModel.Status.Should().Be(200);
            resultModel.Data.Should().NotBeNull();
            resultModel.Data!.Nome.Should().Be("SÃ£o Paulo");
            resultModel.Errors.Should().BeEmpty();
        }

        [Fact(DisplayName = "SaveCity - Deve retornar 400 BadRequest quando ArgumentException Ã© lanÃ§ada")]
        public async Task SaveCity_DeveRetornar400BadRequest_QuandoArgumentExceptionLancada()
        {
            // Arrange
            var inputModel = new RegistroCidadeInputModel
            {
                Nome = "SÃ£o Paulo",
                CodigoIbge = "3550308"
            };

            _cityServiceMock
                .Setup(x => x.SaveCity(It.IsAny<RegistroCidadeInputModel>()))
                .ThrowsAsync(new ArgumentException("Cidade jÃ¡ existe"));

            // Act
            var result = await _controller.SaveCity(inputModel);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            var resultModel = badRequestResult.Value.Should().BeOfType<ResultModel<CidadeModel>>().Subject;
            
            resultModel.Success.Should().BeFalse();
            resultModel.Status.Should().Be(400);
            resultModel.Errors.Should().NotBeEmpty();
            resultModel.Errors.Should().Contain("NÃ£o foi possÃ­vel salvar a Cidade: (SÃ£o Paulo)");
        }

        [Fact(DisplayName = "UpdateCity - Deve retornar 200 OK quando cidade Ã© atualizada com sucesso")]
        public async Task UpdateCity_DeveRetornar200Ok_QuandoCidadeAtualizadaComSucesso()
        {
            // Arrange
            var inputModel = new AlteracaoCidadeInputModel
            {
                Id = 1,
                Nome = "SÃ£o Paulo Atualizado",
                CodigoIbge = "3550308",
                EstadoId = 1
            };

            var cidadeModel = new CidadeModel
            {
                Id = 1,
                Nome = "SÃ£o Paulo Atualizado",
                CodigoIbge = "3550308"
            };

            _cityServiceMock
                .Setup(x => x.UpdateCity(It.IsAny<AlteracaoCidadeInputModel>()))
                .ReturnsAsync(cidadeModel);

            // Act
            var result = await _controller.UpdateCity(inputModel);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var resultModel = okResult.Value.Should().BeOfType<ResultModel<CidadeModel>>().Subject;
            
            resultModel.Success.Should().BeTrue();
            resultModel.Status.Should().Be(200);
            resultModel.Data.Should().NotBeNull();
            resultModel.Data!.Nome.Should().Be("SÃ£o Paulo Atualizado");
        }

        [Fact(DisplayName = "DeleteCity - Deve retornar 200 OK quando cidade Ã© deletada com sucesso")]
        public async Task DeleteCity_DeveRetornar200Ok_QuandoCidadeDeletadaComSucesso()
        {
            // Arrange
            var id = 1;
            var deleteResult = new DeleteResultModel
            {
                Id = id,
                Result = "Removido com sucesso!"
            };

            _cityServiceMock
                .Setup(x => x.DeleteCity(It.IsAny<int>()))
                .ReturnsAsync(deleteResult);

            // Act
            var result = await _controller.DeleteCity(id);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var resultModel = okResult.Value.Should().BeOfType<DeleteResultModel>().Subject;
            
            resultModel.Status.Should().Be(200);
            resultModel.Result.Should().BeNull();
            resultModel.Errors.Should().BeEmpty();
        }

        [Fact(DisplayName = "DeleteCity - Deve retornar 404 NotFound quando FileNotFoundException Ã© lanÃ§ada")]
        public async Task DeleteCity_DeveRetornar404NotFound_QuandoFileNotFoundExceptionLancada()
        {
            // Arrange
            var id = 999;

            _cityServiceMock
                .Setup(x => x.DeleteCity(It.IsAny<int>()))
                .ThrowsAsync(new FileNotFoundException("Cidade nÃ£o encontrada"));

            // Act
            var result = await _controller.DeleteCity(id);

            // Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            var resultModel = notFoundResult.Value.Should().BeOfType<DeleteResultModel>().Subject;
            
            resultModel.Status.Should().Be(404);
            resultModel.Result.Should().Be("NÃ£o deletado");
            resultModel.Errors.Should().NotBeEmpty();
        }
    }
}

