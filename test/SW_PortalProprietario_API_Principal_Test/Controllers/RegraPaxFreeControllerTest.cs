using FluentAssertions;
using Moq;
using SW_PortalCliente_BeachPark.API.src.Controllers.RegraPaxFree;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using System.Net;
using Xunit;

namespace SW_PortalProprietario.Test.Controllers
{
    public class RegraPaxFreeControllerTest
    {
        private readonly Mock<IRegraPaxFreeService> _serviceMock;
        private readonly RegraPaxFreeController _controller;

        public RegraPaxFreeControllerTest()
        {
            _serviceMock = new Mock<IRegraPaxFreeService>();
            _controller = new RegraPaxFreeController(_serviceMock.Object);
        }

        #region SaveRegraPaxFree

        [Fact(DisplayName = "SaveRegraPaxFree - Deve salvar regra com hotéis e retornar 200")]
        public async Task SaveRegraPaxFree_DeveSalvarRegraComHoteisERetornar200()
        {
            // Arrange
            var inputModel = new RegraPaxFreeInputModel
            {
                Nome = "Regra Teste",
                Hoteis = new List<RegraPaxFreeHotelInputModel>
                {
                    new RegraPaxFreeHotelInputModel { HotelId = 1 },
                    new RegraPaxFreeHotelInputModel { HotelId = 2 }
                }
            };

            var resultado = new RegraPaxFreeModel
            {
                Id = 1,
                Nome = "Regra Teste",
                Hoteis = new List<RegraPaxFreeHotelModel>
                {
                    new RegraPaxFreeHotelModel { Id = 1, HotelId = 1 },
                    new RegraPaxFreeHotelModel { Id = 2, HotelId = 2 }
                }
            };

            _serviceMock
                .Setup(x => x.SaveRegraPaxFree(It.IsAny<RegraPaxFreeInputModel>()))
                .ReturnsAsync(resultado);

            // Act
            var result = await _controller.SaveRegraPaxFree(inputModel);

            // Assert
            result.Should().NotBeNull();
            var okResult = result as Microsoft.AspNetCore.Mvc.OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be((int)HttpStatusCode.OK);

            var response = okResult.Value as ResultModel<RegraPaxFreeModel>;
            response.Should().NotBeNull();
            response.Success.Should().BeTrue();
            response.Data.Should().NotBeNull();
            response.Data.Hoteis.Should().HaveCount(2);

            _serviceMock.Verify(x => x.SaveRegraPaxFree(It.Is<RegraPaxFreeInputModel>(m => 
                m.Hoteis.Count == 2)), Times.Once);
        }

        [Fact(DisplayName = "SaveRegraPaxFree - Deve retornar 400 quando validação falhar")]
        public async Task SaveRegraPaxFree_DeveRetornar400QuandoValidacaoFalhar()
        {
            // Arrange
            var inputModel = new RegraPaxFreeInputModel
            {
                Nome = "", // Nome vazio deve causar erro
                Hoteis = new List<RegraPaxFreeHotelInputModel>()
            };

            _serviceMock
                .Setup(x => x.SaveRegraPaxFree(It.IsAny<RegraPaxFreeInputModel>()))
                .ThrowsAsync(new ArgumentException("O Nome da regra deve ser informado"));

            // Act
            var result = await _controller.SaveRegraPaxFree(inputModel);

            // Assert
            result.Should().NotBeNull();
            var badRequestResult = result as Microsoft.AspNetCore.Mvc.BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            var response = badRequestResult.Value as ResultModel<RegraPaxFreeModel>;
            response.Should().NotBeNull();
            response.Success.Should().BeFalse();
            response.Errors.Should().NotBeEmpty();
        }

        #endregion

        #region AlterarRegraPaxFree

        [Fact(DisplayName = "AlterarRegraPaxFree - Deve atualizar regra com hotéis e retornar 200")]
        public async Task AlterarRegraPaxFree_DeveAtualizarRegraComHoteisERetornar200()
        {
            // Arrange
            var inputModel = new AlteracaoRegraPaxFreeInputModel
            {
                Id = 1,
                Nome = "Regra Atualizada",
                Hoteis = new List<RegraPaxFreeHotelInputModel>
                {
                    new RegraPaxFreeHotelInputModel { Id = 1, HotelId = 1 },
                    new RegraPaxFreeHotelInputModel { HotelId = 3 }
                },
                RemoverHoteisNaoEnviados = true
            };

            var resultado = new RegraPaxFreeModel
            {
                Id = 1,
                Nome = "Regra Atualizada",
                Hoteis = new List<RegraPaxFreeHotelModel>
                {
                    new RegraPaxFreeHotelModel { Id = 1, HotelId = 1 },
                    new RegraPaxFreeHotelModel { Id = 3, HotelId = 3 }
                }
            };

            _serviceMock
                .Setup(x => x.UpdateRegraPaxFree(It.IsAny<AlteracaoRegraPaxFreeInputModel>()))
                .ReturnsAsync(resultado);

            // Act
            var result = await _controller.AlterarRegraPaxFree(inputModel);

            // Assert
            result.Should().NotBeNull();
            var okResult = result as Microsoft.AspNetCore.Mvc.OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be((int)HttpStatusCode.OK);

            var response = okResult.Value as ResultModel<RegraPaxFreeModel>;
            response.Should().NotBeNull();
            response.Success.Should().BeTrue();
            response.Data.Hoteis.Should().HaveCount(2);

            _serviceMock.Verify(x => x.UpdateRegraPaxFree(It.Is<AlteracaoRegraPaxFreeInputModel>(m => 
                m.Hoteis.Count == 2 && m.RemoverHoteisNaoEnviados == true)), Times.Once);
        }

        [Fact(DisplayName = "AlterarRegraPaxFree - Deve remover todos os hotéis quando lista vazia")]
        public async Task AlterarRegraPaxFree_DeveRemoverTodosOsHoteisQuandoListaVazia()
        {
            // Arrange
            var inputModel = new AlteracaoRegraPaxFreeInputModel
            {
                Id = 1,
                Nome = "Regra sem Hotéis",
                Hoteis = new List<RegraPaxFreeHotelInputModel>(),
                RemoverHoteisNaoEnviados = true
            };

            var resultado = new RegraPaxFreeModel
            {
                Id = 1,
                Nome = "Regra sem Hotéis",
                Hoteis = new List<RegraPaxFreeHotelModel>()
            };

            _serviceMock
                .Setup(x => x.UpdateRegraPaxFree(It.IsAny<AlteracaoRegraPaxFreeInputModel>()))
                .ReturnsAsync(resultado);

            // Act
            var result = await _controller.AlterarRegraPaxFree(inputModel);

            // Assert
            result.Should().NotBeNull();
            var okResult = result as Microsoft.AspNetCore.Mvc.OkObjectResult;
            okResult.Should().NotBeNull();

            var response = okResult.Value as ResultModel<RegraPaxFreeModel>;
            response.Data.Hoteis.Should().BeEmpty();

            _serviceMock.Verify(x => x.UpdateRegraPaxFree(It.Is<AlteracaoRegraPaxFreeInputModel>(m => 
                m.Hoteis.Count == 0 && m.RemoverHoteisNaoEnviados == true)), Times.Once);
        }

        #endregion

        #region Search

        [Fact(DisplayName = "Search - Deve retornar regras com hotéis")]
        public async Task Search_DeveRetornarRegrasComHoteis()
        {
            // Arrange
            var resultado = new List<RegraPaxFreeModel>
            {
                new RegraPaxFreeModel
                {
                    Id = 1,
                    Nome = "Regra 1",
                    Hoteis = new List<RegraPaxFreeHotelModel>
                    {
                        new RegraPaxFreeHotelModel { Id = 1, HotelId = 1 },
                        new RegraPaxFreeHotelModel { Id = 2, HotelId = 2 }
                    }
                },
                new RegraPaxFreeModel
                {
                    Id = 2,
                    Nome = "Regra 2",
                    Hoteis = new List<RegraPaxFreeHotelModel>()
                }
            };

            _serviceMock
                .Setup(x => x.Search(It.IsAny<SearchPadraoModel>()))
                .ReturnsAsync(resultado);

            // Act
            var result = await _controller.Search(new SearchPadraoModel());

            // Assert
            result.Should().NotBeNull();
            var okResult = result as Microsoft.AspNetCore.Mvc.OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be((int)HttpStatusCode.OK);

            var response = okResult.Value as ResultModel<List<RegraPaxFreeModel>>;
            response.Should().NotBeNull();
            response.Success.Should().BeTrue();
            response.Data.Should().HaveCount(2);
            response.Data[0].Hoteis.Should().HaveCount(2);
            response.Data[1].Hoteis.Should().BeEmpty();
        }

        [Fact(DisplayName = "Search - Deve retornar 404 quando nenhuma regra encontrada")]
        public async Task Search_DeveRetornar404QuandoNenhumaRegraEncontrada()
        {
            // Arrange
            _serviceMock
                .Setup(x => x.Search(It.IsAny<SearchPadraoModel>()))
                .ReturnsAsync(new List<RegraPaxFreeModel>());

            // Act
            var result = await _controller.Search(new SearchPadraoModel());

            // Assert
            result.Should().NotBeNull();
            var okResult = result as Microsoft.AspNetCore.Mvc.OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be((int)HttpStatusCode.OK);

            var response = okResult.Value as ResultModel<List<RegraPaxFreeModel>>;
            response.Status.Should().Be((int)HttpStatusCode.NotFound);
            response.Data.Should().BeEmpty();
            response.Errors.Should().Contain("Ops! Nenhum registro encontrado!");
        }

        #endregion

        #region DeleteRegraPaxFree

        [Fact(DisplayName = "DeleteRegraPaxFree - Deve deletar regra com sucesso")]
        public async Task DeleteRegraPaxFree_DeveDeletarRegraComSucesso()
        {
            // Arrange
            var deleteResult = new DeleteResultModel
            {
                Id = 1,
                Result = "Removido com sucesso!",
                Status = (int)HttpStatusCode.OK
            };

            _serviceMock
                .Setup(x => x.DeleteRegraPaxFree(It.IsAny<int>()))
                .ReturnsAsync(deleteResult);

            // Act
            var result = await _controller.DeleteRegraPaxFree(1);

            // Assert
            result.Should().NotBeNull();
            var okResult = result as Microsoft.AspNetCore.Mvc.OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be((int)HttpStatusCode.OK);

            var response = okResult.Value as DeleteResultModel;
            response.Should().NotBeNull();
            response.Result.Should().Be("Removido com sucesso!");

            _serviceMock.Verify(x => x.DeleteRegraPaxFree(1), Times.Once);
        }

        #endregion
    }
}

