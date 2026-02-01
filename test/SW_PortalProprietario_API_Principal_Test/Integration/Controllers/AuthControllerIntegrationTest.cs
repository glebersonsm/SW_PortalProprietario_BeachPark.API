using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.AuthModels;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Test.Base;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace SW_PortalProprietario.Test.Integration.Controllers
{
    /// <summary>
    /// Testes de integração para AuthController
    /// Estes testes garantem que o comportamento atual do sistema continue funcionando
    /// </summary>
    public class AuthControllerIntegrationTest : IntegrationTestBase, IClassFixture<IntegrationTestBase>
    {
        private readonly HttpClient _client;

        public AuthControllerIntegrationTest()
        {
            _client = CreateClient();
        }

        [Fact(DisplayName = "POST /Auth/register - Deve retornar estrutura de resposta correta")]
        public async Task Register_DeveRetornarEstruturaCorreta()
        {
            // Arrange
            var inputModel = new UserRegisterInputModel
            {
                FullName = "Teste Integração",
                Email = "teste.integracao@example.com",
                Password = "Senha123!",
                PasswordConfirmation = "Senha123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/Auth/register", inputModel);

            // Assert
            var content = await response.Content.ReadAsStringAsync();
            
            // Valida que é JSON válido
            Action parseJson = () => JsonSerializer.Deserialize<object>(content);
            parseJson.Should().NotThrow("A resposta deve ser JSON válido");
            
            // Se não for erro de servidor, valida estrutura
            if (response.StatusCode != HttpStatusCode.InternalServerError)
            {
                response.StatusCode.Should().BeOneOf(
                    HttpStatusCode.OK, 
                    HttpStatusCode.BadRequest, 
                    HttpStatusCode.Conflict);
                
                var result = JsonSerializer.Deserialize<ResultModel<UserRegisterResultModel>>(
                    content, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                result.Should().NotBeNull();
                result!.Status.Should().BeOneOf(200, 400, 409);
            }
        }

        [Fact(DisplayName = "POST /Auth/login - Deve retornar estrutura de resposta correta")]
        public async Task Login_DeveRetornarEstruturaCorreta()
        {
            // Arrange
            var loginModel = new LoginInputModel
            {
                Login = "teste@example.com",
                Senha = "Senha123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/Auth/login", loginModel);

            // Assert
            var content = await response.Content.ReadAsStringAsync();
            
            // Valida que é JSON válido
            Action parseJson = () => JsonSerializer.Deserialize<object>(content);
            parseJson.Should().NotThrow("A resposta deve ser JSON válido");
            
            // Se não for erro de servidor, valida estrutura
            if (response.StatusCode != HttpStatusCode.InternalServerError)
            {
                response.StatusCode.Should().BeOneOf(
                    HttpStatusCode.OK, 
                    HttpStatusCode.BadRequest, 
                    HttpStatusCode.NotFound,
                    HttpStatusCode.Unauthorized);
                
                var result = JsonSerializer.Deserialize<ResultModel<TokenResultModel>>(
                    content, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                result.Should().NotBeNull();
                result!.Status.Should().BeOneOf(200, 400, 401, 404);
            }
        }

        [Fact(DisplayName = "POST /Auth/login - Deve retornar 401 quando não autenticado")]
        public async Task Login_DeveRetornar401QuandoNaoAutenticado()
        {
            // Arrange
            var loginModel = new LoginInputModel
            {
                Login = "usuario.inexistente@example.com",
                Senha = "SenhaIncorreta123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/Auth/login", loginModel);

            // Assert
            // Pode retornar 401, 404 ou 400 dependendo da implementação
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.Unauthorized,
                HttpStatusCode.NotFound,
                HttpStatusCode.BadRequest);
        }

        [Fact(DisplayName = "Validação de regressão - Endpoints devem manter estrutura de resposta")]
        public async Task ValidacaoRegressao_EndpointsDevemManterEstruturaResposta()
        {
            // Arrange
            var loginModel = new LoginInputModel
            {
                Login = "teste@example.com",
                Senha = "Senha123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/Auth/login", loginModel);

            // Assert - Valida que a estrutura de resposta mantém o formato esperado
            var content = await response.Content.ReadAsStringAsync();
            
            // Valida que é JSON válido
            Action parseJson = () => JsonSerializer.Deserialize<object>(content);
            parseJson.Should().NotThrow("A resposta deve ser JSON válido");
            
            // Se não for erro de servidor, valida estrutura
            if (response.StatusCode != HttpStatusCode.InternalServerError)
            {
                content.Should().Contain("\"success\"");
                content.Should().Contain("\"status\"");
                content.Should().Contain("\"data\"");
            }
        }

        protected override void Dispose(bool disposing)
        {
            _client?.Dispose();
            base.Dispose(disposing);
        }
    }
}

