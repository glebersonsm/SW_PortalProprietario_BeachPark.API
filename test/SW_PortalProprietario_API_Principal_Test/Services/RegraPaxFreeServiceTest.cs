using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Services.Core;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Domain.Entities.Core.Geral;
using SW_Utils.Auxiliar;
using Xunit;

namespace SW_PortalProprietario.Test.Services
{
    public class RegraPaxFreeServiceTest
    {
        private readonly Mock<IRepositoryNH> _repositoryMock;
        private readonly Mock<ILogger<RegraPaxFreeService>> _loggerMock;
        private readonly Mock<IServiceBase> _serviceBaseMock;
        private readonly Mock<IProjectObjectMapper> _mapperMock;
        private readonly RegraPaxFreeService _service;

        public RegraPaxFreeServiceTest()
        {
            _repositoryMock = new Mock<IRepositoryNH>();
            _loggerMock = new Mock<ILogger<RegraPaxFreeService>>();
            _serviceBaseMock = new Mock<IServiceBase>();
            _mapperMock = new Mock<IProjectObjectMapper>();

            _service = new RegraPaxFreeService(
                _repositoryMock.Object,
                _loggerMock.Object,
                _mapperMock.Object,
                _serviceBaseMock.Object
            );
        }

        #region SaveRegraPaxFree com Hotéis

        [Fact(DisplayName = "SaveRegraPaxFree - Deve salvar regra com hotéis vinculados")]
        public async Task SaveRegraPaxFree_DeveSalvarRegraComHoteisVinculados()
        {
            // Arrange
            var inputModel = new RegraPaxFreeInputModel
            {
                Nome = "Regra Teste com Hotéis",
                DataInicioVigencia = DateTime.Now,
                DataFimVigencia = DateTime.Now.AddYears(1),
                Hoteis = new List<RegraPaxFreeHotelInputModel>
                {
                    new RegraPaxFreeHotelInputModel { HotelId = 1 },
                    new RegraPaxFreeHotelInputModel { HotelId = 2 },
                    new RegraPaxFreeHotelInputModel { HotelId = 3 }
                },
                Configuracoes = new List<RegraPaxFreeConfiguracaoInputModel>
                {
                    new RegraPaxFreeConfiguracaoInputModel
                    {
                        QuantidadeAdultos = 2,
                        QuantidadePessoasFree = 1,
                        IdadeMaximaAnos = 12,
                        TipoOperadorIdade = "<=",
                        TipoDataReferencia = "RESERVA"
                    }
                }
            };

            var regraSalva = new RegraPaxFree
            {
                Id = 1,
                Nome = inputModel.Nome,
                DataInicioVigencia = inputModel.DataInicioVigencia,
                DataFimVigencia = inputModel.DataFimVigencia
            };

            var regraModel = new RegraPaxFreeModel
            {
                Id = 1,
                Nome = inputModel.Nome,
                Configuracoes = new List<RegraPaxFreeConfiguracaoModel>(),
                Hoteis = new List<RegraPaxFreeHotelModel>
                {
                    new RegraPaxFreeHotelModel { Id = 1, HotelId = 1 },
                    new RegraPaxFreeHotelModel { Id = 2, HotelId = 2 },
                    new RegraPaxFreeHotelModel { Id = 3, HotelId = 3 }
                }
            };

            _repositoryMock
                .Setup(x => x.FindByHql<RegraPaxFree>(It.IsAny<string>()))
                .ReturnsAsync(new List<RegraPaxFree>());

            _repositoryMock
                .Setup(x => x.Save(It.IsAny<RegraPaxFree>()))
                .ReturnsAsync(regraSalva);

            _repositoryMock
                .Setup(x => x.GetLoggedUser())
                .ReturnsAsync((("1", "provider", "1", false)) as (string userId, string providerKeyUser, string companyId, bool isAdm)?);

            _repositoryMock
                .Setup(x => x.FindByHql<RegraPaxFreeHotel>(It.IsAny<string>()))
                .ReturnsAsync(new List<RegraPaxFreeHotel>());

            _repositoryMock
                .Setup(x => x.CommitAsync())
                .ReturnsAsync((true, (Exception?)null));

            _repositoryMock
                .Setup(x => x.FindByHql<RegraPaxFreeConfiguracao>(It.IsAny<string>()))
                .ReturnsAsync(new List<RegraPaxFreeConfiguracao>());

            _mapperMock
                .Setup(x => x.Map(It.IsAny<RegraPaxFree>(), It.IsAny<RegraPaxFreeModel>()))
                .Returns(regraModel);

            _serviceBaseMock
                .Setup(x => x.SetUserName(It.IsAny<List<RegraPaxFreeModel>>()))
                .ReturnsAsync(new List<RegraPaxFreeModel> { regraModel });

            // Act
            var result = await _service.SaveRegraPaxFree(inputModel);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(1);

            _repositoryMock.Verify(x => x.BeginTransaction(), Times.Once);
            _repositoryMock.Verify(x => x.Save(It.IsAny<RegraPaxFree>()), Times.Once);
            _repositoryMock.Verify(x => x.Save(It.IsAny<RegraPaxFreeHotel>()), Times.Exactly(3));
            _repositoryMock.Verify(x => x.CommitAsync(), Times.Once);
        }

        [Fact(DisplayName = "SaveRegraPaxFree - Deve salvar regra sem hotéis")]
        public async Task SaveRegraPaxFree_DeveSalvarRegraSemHoteis()
        {
            // Arrange
            var inputModel = new RegraPaxFreeInputModel
            {
                Nome = "Regra Teste sem Hotéis",
                Hoteis = new List<RegraPaxFreeHotelInputModel>()
            };

            var regraSalva = new RegraPaxFree { Id = 1, Nome = inputModel.Nome };
            var regraModel = new RegraPaxFreeModel { Id = 1, Nome = inputModel.Nome, Hoteis = new List<RegraPaxFreeHotelModel>() };

            _repositoryMock
                .Setup(x => x.FindByHql<RegraPaxFree>(It.IsAny<string>()))
                .ReturnsAsync(new List<RegraPaxFree>());

            _repositoryMock
                .Setup(x => x.Save(It.IsAny<RegraPaxFree>()))
                .ReturnsAsync(regraSalva);

            _repositoryMock
                .Setup(x => x.CommitAsync())
                .ReturnsAsync((true, (Exception?)null));

            _repositoryMock
                .Setup(x => x.FindByHql<RegraPaxFreeConfiguracao>(It.IsAny<string>()))
                .ReturnsAsync(new List<RegraPaxFreeConfiguracao>());

            _mapperMock
                .Setup(x => x.Map(It.IsAny<RegraPaxFree>(), It.IsAny<RegraPaxFreeModel>()))
                .Returns(regraModel);

            _serviceBaseMock
                .Setup(x => x.SetUserName(It.IsAny<List<RegraPaxFreeModel>>()))
                .ReturnsAsync(new List<RegraPaxFreeModel> { regraModel });

            // Act
            var result = await _service.SaveRegraPaxFree(inputModel);

            // Assert
            result.Should().NotBeNull();
            result.Hoteis.Should().NotBeNull();
            result.Hoteis.Should().BeEmpty();

            _repositoryMock.Verify(x => x.Save(It.IsAny<RegraPaxFreeHotel>()), Times.Never);
        }

        #endregion

        #region UpdateRegraPaxFree com Hotéis

        [Fact(DisplayName = "UpdateRegraPaxFree - Deve atualizar regra adicionando novos hotéis")]
        public async Task UpdateRegraPaxFree_DeveAtualizarRegraAdicionandoNovosHoteis()
        {
            // Arrange
            var regraId = 1;
            var regraExistente = new RegraPaxFree
            {
                Id = regraId,
                Nome = "Regra Existente",
                DataHoraRemocao = null,
                UsuarioRemocao = null
            };

            var inputModel = new AlteracaoRegraPaxFreeInputModel
            {
                Id = regraId,
                Nome = "Regra Atualizada",
                Hoteis = new List<RegraPaxFreeHotelInputModel>
                {
                    new RegraPaxFreeHotelInputModel { HotelId = 1 },
                    new RegraPaxFreeHotelInputModel { HotelId = 2 }
                },
                RemoverHoteisNaoEnviados = true
            };

            var regraModel = new RegraPaxFreeModel
            {
                Id = regraId,
                Nome = inputModel.Nome,
                Hoteis = new List<RegraPaxFreeHotelModel>
                {
                    new RegraPaxFreeHotelModel { Id = 1, HotelId = 1 },
                    new RegraPaxFreeHotelModel { Id = 2, HotelId = 2 }
                }
            };

            _repositoryMock
                .Setup(x => x.FindByHql<RegraPaxFree>(It.IsAny<string>()))
                .ReturnsAsync(new List<RegraPaxFree> { regraExistente });

            _repositoryMock
                .Setup(x => x.Save(It.IsAny<RegraPaxFree>()))
                .ReturnsAsync(regraExistente);

            _repositoryMock
                .Setup(x => x.GetLoggedUser())
                .ReturnsAsync((("1", "provider", "1", false)) as (string userId, string providerKeyUser, string companyId, bool isAdm)?);

            _repositoryMock
                .Setup(x => x.FindByHql<RegraPaxFreeHotel>(It.IsAny<string>()))
                .ReturnsAsync(new List<RegraPaxFreeHotel>());

            _repositoryMock
                .Setup(x => x.CommitAsync())
                .ReturnsAsync((true, (Exception?)null));

            _repositoryMock
                .Setup(x => x.FindByHql<RegraPaxFreeConfiguracao>(It.IsAny<string>()))
                .ReturnsAsync(new List<RegraPaxFreeConfiguracao>());

            _mapperMock
                .Setup(x => x.Map(It.IsAny<RegraPaxFree>(), It.IsAny<RegraPaxFreeModel>()))
                .Returns(regraModel);

            _serviceBaseMock
                .Setup(x => x.SetUserName(It.IsAny<List<RegraPaxFreeModel>>()))
                .ReturnsAsync(new List<RegraPaxFreeModel> { regraModel });

            // Act
            var result = await _service.UpdateRegraPaxFree(inputModel);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(regraId);
            result.Hoteis.Should().NotBeNull();
            result.Hoteis.Should().HaveCount(2);

            _repositoryMock.Verify(x => x.BeginTransaction(), Times.Once);
            _repositoryMock.Verify(x => x.Save(It.IsAny<RegraPaxFree>()), Times.Once);
            _repositoryMock.Verify(x => x.Save(It.IsAny<RegraPaxFreeHotel>()), Times.Exactly(2));
            _repositoryMock.Verify(x => x.CommitAsync(), Times.Once);
        }

        [Fact(DisplayName = "UpdateRegraPaxFree - Deve remover todos os hotéis quando lista vazia")]
        public async Task UpdateRegraPaxFree_DeveRemoverTodosOsHoteisQuandoListaVazia()
        {
            // Arrange
            var regraId = 1;
            var regraExistente = new RegraPaxFree
            {
                Id = regraId,
                Nome = "Regra com Hotéis",
                DataHoraRemocao = null,
                UsuarioRemocao = null
            };

            var hotelExistente = new RegraPaxFreeHotel
            {
                Id = 1,
                RegraPaxFree = regraExistente,
                HotelId = 1,
                DataHoraRemocao = null,
                UsuarioRemocao = null
            };

            var inputModel = new AlteracaoRegraPaxFreeInputModel
            {
                Id = regraId,
                Nome = "Regra sem Hotéis",
                Hoteis = new List<RegraPaxFreeHotelInputModel>(),
                RemoverHoteisNaoEnviados = true
            };

            var regraModel = new RegraPaxFreeModel
            {
                Id = regraId,
                Nome = inputModel.Nome,
                Hoteis = new List<RegraPaxFreeHotelModel>()
            };

            _repositoryMock
                .Setup(x => x.FindByHql<RegraPaxFree>(It.IsAny<string>()))
                .ReturnsAsync(new List<RegraPaxFree> { regraExistente });

            _repositoryMock
                .Setup(x => x.Save(It.IsAny<RegraPaxFree>()))
                .ReturnsAsync(regraExistente);

            _repositoryMock
                .Setup(x => x.GetLoggedUser())
                .ReturnsAsync((("1", "provider", "1", false)) as (string userId, string providerKeyUser, string companyId, bool isAdm)?);

            _repositoryMock
                .Setup(x => x.FindByHql<RegraPaxFreeHotel>(It.Is<string>(s => s.Contains("UsuarioRemocao is null"))))
                .ReturnsAsync(new List<RegraPaxFreeHotel> { hotelExistente });

            _repositoryMock
                .Setup(x => x.FindByHql<RegraPaxFreeHotel>(It.Is<string>(s => !s.Contains("UsuarioRemocao is null"))))
                .ReturnsAsync(new List<RegraPaxFreeHotel>());

            _repositoryMock
                .Setup(x => x.CommitAsync())
                .ReturnsAsync((true, (Exception?)null));

            _repositoryMock
                .Setup(x => x.FindByHql<RegraPaxFreeConfiguracao>(It.IsAny<string>()))
                .ReturnsAsync(new List<RegraPaxFreeConfiguracao>());

            _mapperMock
                .Setup(x => x.Map(It.IsAny<RegraPaxFree>(), It.IsAny<RegraPaxFreeModel>()))
                .Returns(regraModel);

            _serviceBaseMock
                .Setup(x => x.SetUserName(It.IsAny<List<RegraPaxFreeModel>>()))
                .ReturnsAsync(new List<RegraPaxFreeModel> { regraModel });

            // Act
            var result = await _service.UpdateRegraPaxFree(inputModel);

            // Assert
            result.Should().NotBeNull();
            result.Hoteis.Should().NotBeNull();
            result.Hoteis.Should().BeEmpty();

            // Verificar se o hotel foi marcado para remoção (soft delete)
            _repositoryMock.Verify(x => x.Save(It.Is<RegraPaxFreeHotel>(h => h.UsuarioRemocao != null && h.DataHoraRemocao != null)), Times.Once);
        }


        #endregion

        #region Search com Hotéis

        [Fact(DisplayName = "Search - Deve retornar regras com hotéis vinculados")]
        public async Task Search_DeveRetornarRegrasComHoteisVinculados()
        {
            // Arrange
            var regra1 = new RegraPaxFree
            {
                Id = 1,
                Nome = "Regra 1",
                DataHoraRemocao = null,
                UsuarioRemocao = null
            };

            var hotel1 = new RegraPaxFreeHotel
            {
                Id = 1,
                RegraPaxFree = regra1,
                HotelId = 1,
                DataHoraRemocao = null,
                UsuarioRemocao = null
            };


            var regraModel1 = new RegraPaxFreeModel { Id = 1, Nome = "Regra 1" };

            _repositoryMock
                .Setup(x => x.FindByHql<RegraPaxFree>(It.IsAny<string>(), It.IsAny<Parameter[]>()))
                .ReturnsAsync(new List<RegraPaxFree> { regra1 });

            _mapperMock
                .Setup(x => x.Map(regra1, It.IsAny<RegraPaxFreeModel>()))
                .Returns(regraModel1);

 
            _repositoryMock
                .Setup(x => x.FindByHql<RegraPaxFreeConfiguracao>(It.IsAny<string>()))
                .ReturnsAsync(new List<RegraPaxFreeConfiguracao>());

            _repositoryMock
                .Setup(x => x.FindByHql<RegraPaxFreeHotel>(It.Is<string>(s => s.Contains("r.Id = 1"))))
                .ReturnsAsync(new List<RegraPaxFreeHotel> { hotel1 });

            _repositoryMock
                .Setup(x => x.FindByHql<RegraPaxFreeHotel>(It.Is<string>(s => s.Contains("r.Id = 2"))))
                .ReturnsAsync(new List<RegraPaxFreeHotel>());

            _serviceBaseMock
                .Setup(x => x.SetUserName(It.IsAny<List<RegraPaxFreeModel>>()))
                .ReturnsAsync((List<RegraPaxFreeModel> models) =>
                {
                    models[0].Hoteis = new List<RegraPaxFreeHotelModel>
                    {
                        new RegraPaxFreeHotelModel { Id = 1, HotelId = 1 },
                    };
                    return models;
                });

            // Act
            var result = await _service.Search(new SearchPadraoModel());

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            var regraModelResult1 = result!.First(r => r.Id == 1);
            regraModelResult1.Hoteis.Should().NotBeNull();
            regraModelResult1.Hoteis.Should().HaveCount(1);

        }

        #endregion

        #region GetRegraVigente com Hotéis

        [Fact(DisplayName = "GetRegraVigente - Deve retornar regra vigente com hotéis vinculados")]
        public async Task GetRegraVigente_DeveRetornarRegraVigenteComHoteisVinculados()
        {
            // Arrange
            var hoje = DateTime.Today;
            var regraVigente = new RegraPaxFree
            {
                Id = 1,
                Nome = "Regra Vigente",
                DataInicioVigencia = hoje.AddDays(-10),
                DataFimVigencia = hoje.AddDays(10),
                DataHoraRemocao = null,
                UsuarioRemocao = null
            };

            var hotel1 = new RegraPaxFreeHotel
            {
                Id = 1,
                RegraPaxFree = regraVigente,
                HotelId = 1,
                DataHoraRemocao = null,
                UsuarioRemocao = null
            };

            var regraModel = new RegraPaxFreeModel
            {
                Id = 1,
                Nome = "Regra Vigente",
                DataInicioVigencia = regraVigente.DataInicioVigencia,
                DataFimVigencia = regraVigente.DataFimVigencia
            };

            _repositoryMock
                .Setup(x => x.FindByHql<RegraPaxFree>(It.IsAny<string>(), It.IsAny<Parameter[]>()))
                .ReturnsAsync(new List<RegraPaxFree> { regraVigente });

            _mapperMock
                .Setup(x => x.Map(regraVigente, It.IsAny<RegraPaxFreeModel>()))
                .Returns(regraModel);

            _repositoryMock
                .Setup(x => x.FindByHql<RegraPaxFreeConfiguracao>(It.IsAny<string>()))
                .ReturnsAsync(new List<RegraPaxFreeConfiguracao>());

            _repositoryMock
                .Setup(x => x.FindByHql<RegraPaxFreeHotel>(It.IsAny<string>()))
                .ReturnsAsync(new List<RegraPaxFreeHotel> { hotel1 });

            // Act
            var result = await _service.GetRegraVigente();

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result.Nome.Should().Be("Regra Vigente");
            result.Hoteis.Should().NotBeNull();
            result.Hoteis.Should().HaveCount(1);
            result.Hoteis!.First().HotelId.Should().Be(1);
        }

        #endregion

        #region Validações

        [Fact(DisplayName = "SaveRegraPaxFree - Deve ignorar hotéis com HotelId inválido")]
        public async Task SaveRegraPaxFree_DeveIgnorarHoteisComHotelIdInvalido()
        {
            // Arrange
            var inputModel = new RegraPaxFreeInputModel
            {
                Nome = "Regra Teste",
                Hoteis = new List<RegraPaxFreeHotelInputModel>
                {
                    new RegraPaxFreeHotelInputModel { HotelId = 1 }, // Válido
                    new RegraPaxFreeHotelInputModel { HotelId = null }, // Inválido - deve ser ignorado
                    new RegraPaxFreeHotelInputModel { HotelId = 0 }, // Inválido - deve ser ignorado
                    new RegraPaxFreeHotelInputModel { HotelId = -1 } // Inválido - deve ser ignorado
                }
            };

            var regraSalva = new RegraPaxFree { Id = 1, Nome = inputModel.Nome };
            var regraModel = new RegraPaxFreeModel { Id = 1, Nome = inputModel.Nome };

            _repositoryMock
                .Setup(x => x.FindByHql<RegraPaxFree>(It.IsAny<string>()))
                .ReturnsAsync(new List<RegraPaxFree>());

            _repositoryMock
                .Setup(x => x.Save(It.IsAny<RegraPaxFree>()))
                .ReturnsAsync(regraSalva);

            _repositoryMock
                .Setup(x => x.GetLoggedUser())
                .ReturnsAsync((("1", "provider", "1", false)) as (string userId, string providerKeyUser, string companyId, bool isAdm)?);

            _repositoryMock
                .Setup(x => x.FindByHql<RegraPaxFreeHotel>(It.IsAny<string>()))
                .ReturnsAsync(new List<RegraPaxFreeHotel>());

            _repositoryMock
                .Setup(x => x.CommitAsync())
                .ReturnsAsync((true, (Exception?)null));

            _repositoryMock
                .Setup(x => x.FindByHql<RegraPaxFreeConfiguracao>(It.IsAny<string>()))
                .ReturnsAsync(new List<RegraPaxFreeConfiguracao>());

            _mapperMock
                .Setup(x => x.Map(It.IsAny<RegraPaxFree>(), It.IsAny<RegraPaxFreeModel>()))
                .Returns(regraModel);

            _serviceBaseMock
                .Setup(x => x.SetUserName(It.IsAny<List<RegraPaxFreeModel>>()))
                .ReturnsAsync(new List<RegraPaxFreeModel> { regraModel });

            // Act
            var result = await _service.SaveRegraPaxFree(inputModel);

            // Assert
            result.Should().NotBeNull();
            // Apenas 1 hotel válido deve ser salvo
            _repositoryMock.Verify(x => x.Save(It.Is<RegraPaxFreeHotel>(h => h.HotelId == 1)), Times.Once);
            _repositoryMock.Verify(x => x.Save(It.Is<RegraPaxFreeHotel>(h => h.HotelId == null || h.HotelId <= 0)), Times.Never);
        }

        #endregion
    }
}

