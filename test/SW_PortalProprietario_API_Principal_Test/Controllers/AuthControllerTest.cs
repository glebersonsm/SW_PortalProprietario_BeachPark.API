using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SW_PortalProprietario.API.src.Controllers;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.AuthModels;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using Xunit;

namespace SW_PortalProprietario.Test.Controllers
{
    public class AuthControllerTest
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly Mock<ILogger<AuthController>> _loggerMock;
        private readonly AuthController _controller;

        public AuthControllerTest()
        {
            _authServiceMock = new Mock<IAuthService>();
            _loggerMock = new Mock<ILogger<AuthController>>();
            _controller = new AuthController(_authServiceMock.Object, _loggerMock.Object);
        }

        [Fact(DisplayName = "Register - Deve retornar 200 OK quando usuário é registrado com sucesso")]
        public async Task Register_DeveRetornar200Ok_QuandoUsuarioRegistradoComSucesso()
        {
            // Arrange
            var inputModel = new UserRegisterInputModel
            {
                FullName = "João Silva",
                Email = "joao@example.com",
                Password = "Senha123!",
                PasswordConfirmation = "Senha123!"
            };

            var registerResult = new UserRegisterResultModel
            {
                Id = 1,
                Login = "joao@example.com"
            };

            _authServiceMock
                .Setup(x => x.Register(It.IsAny<UserRegisterInputModel>()))
                .ReturnsAsync(registerResult);

            // Act
            var result = await _controller.Register(inputModel);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var resultModel = okResult.Value.Should().BeOfType<ResultModel<UserRegisterResultModel>>().Subject;
            
            resultModel.Success.Should().BeTrue();
            resultModel.Status.Should().Be(200);
            resultModel.Data.Should().NotBeNull();
            resultModel.Data!.Login.Should().Be("joao@example.com");
            resultModel.Errors.Should().BeEmpty();
        }

        [Fact(DisplayName = "Register - Deve retornar 400 BadRequest quando ArgumentException é lançada")]
        public async Task Register_DeveRetornar400BadRequest_QuandoArgumentExceptionLancada()
        {
            // Arrange
            var inputModel = new UserRegisterInputModel
            {
                FullName = "João Silva",
                Email = "joao@example.com",
                Password = "Senha123!",
                PasswordConfirmation = "Senha123!"
            };

            _authServiceMock
                .Setup(x => x.Register(It.IsAny<UserRegisterInputModel>()))
                .ThrowsAsync(new ArgumentException("Email já está em uso"));

            // Act
            var result = await _controller.Register(inputModel);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            var resultModel = badRequestResult.Value.Should().BeOfType<ResultModel<UserRegisterResultModel>>().Subject;
            
            resultModel.Success.Should().BeFalse();
            resultModel.Status.Should().Be(400);
            resultModel.Errors.Should().NotBeEmpty();
            resultModel.Errors.Should().Contain("Não foi possível registrar o usuário: (João Silva)");
        }

        [Fact(DisplayName = "Login - Deve retornar 200 OK quando login é bem-sucedido")]
        public async Task Login_DeveRetornar200Ok_QuandoLoginBemSucedido()
        {
            // Arrange
            var loginModel = new LoginInputModel
            {
                Login = "joao@example.com",
                Senha = "Senha123!"
            };

            var tokenResult = new TokenResultModel
            {
                Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
                FimValidade = DateTime.Now.AddSeconds(3600)
            };

            _authServiceMock
                .Setup(x => x.Login(It.IsAny<LoginInputModel>()))
                .ReturnsAsync(tokenResult);

            // Act
            var result = await _controller.Login(loginModel);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var resultModel = okResult.Value.Should().BeOfType<ResultModel<TokenResultModel>>().Subject;
            
            resultModel.Success.Should().BeTrue();
            resultModel.Status.Should().Be(200);
            resultModel.Data.Should().NotBeNull();
            resultModel.Data!.Token.Should().NotBeNullOrEmpty();
            resultModel.Errors.Should().BeEmpty();
            
            // Verifica se a senha foi limpa
            loginModel.Senha.Should().BeEmpty();
        }

        [Fact(DisplayName = "Login - Deve retornar 404 NotFound quando FileNotFoundException é lançada")]
        public async Task Login_DeveRetornar404NotFound_QuandoFileNotFoundExceptionLancada()
        {
            // Arrange
            var loginModel = new LoginInputModel
            {
                Login = "naoexiste@example.com",
                Senha = "Senha123!"
            };

            _authServiceMock
                .Setup(x => x.Login(It.IsAny<LoginInputModel>()))
                .ThrowsAsync(new FileNotFoundException("Usuário não encontrado"));

            // Act
            var result = await _controller.Login(loginModel);

            // Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            var resultModel = notFoundResult.Value.Should().BeOfType<ResultModel<TokenResultModel>>().Subject;
            
            resultModel.Success.Should().BeFalse();
            resultModel.Status.Should().Be(404);
            resultModel.Errors.Should().NotBeEmpty();
            resultModel.Errors.Should().Contain("Usuário não encontrado");
        }

        [Fact(DisplayName = "Login - Deve retornar 400 BadRequest quando ArgumentException é lançada")]
        public async Task Login_DeveRetornar400BadRequest_QuandoArgumentExceptionLancada()
        {
            // Arrange
            var loginModel = new LoginInputModel
            {
                Login = "joao@example.com",
                Senha = "SenhaIncorreta"
            };

            _authServiceMock
                .Setup(x => x.Login(It.IsAny<LoginInputModel>()))
                .ThrowsAsync(new ArgumentException("Senha incorreta"));

            // Act
            var result = await _controller.Login(loginModel);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            var resultModel = badRequestResult.Value.Should().BeOfType<ResultModel<TokenResultModel>>().Subject;
            
            resultModel.Success.Should().BeFalse();
            resultModel.Status.Should().Be(400);
            resultModel.Errors.Should().NotBeEmpty();
            resultModel.Errors.Should().Contain("Senha incorreta");
        }
    }
}

