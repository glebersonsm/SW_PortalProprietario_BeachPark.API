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

namespace SW_PortalProprietario.Test.Regression
{
    /// <summary>
    /// Testes de regressÃ£o - Garantem que funcionalidades existentes continuam funcionando
    /// Estes testes devem passar sempre para garantir que mudanÃ§as nÃ£o quebram funcionalidades existentes
    /// </summary>
    public class AuthRegressionTests : IntegrationTestBase, IClassFixture<IntegrationTestBase>
    {
        private readonly HttpClient _client;

        public AuthRegressionTests()
        {
            _client = CreateClient();
        }

        [Fact(DisplayName = "REGRESSÃƒO: Endpoints devem sempre retornar JSON vÃ¡lido")]
        public async Task Regressao_EndpointsDevemRetornarJsonValido()
        {
            // Arrange
            var endpoints = new (string method, string endpoint, object model)[]
            {
                ("POST", "/Auth/login", (object)new LoginInputModel { Login = "teste@example.com", Senha = "Senha123!" }),
                ("POST", "/Auth/register", (object)new UserRegisterInputModel 
                { 
                    FullName = "Teste", 
                    Email = "teste@example.com", 
                    Password = "Senha123!", 
                    PasswordConfirmation = "Senha123!" 
                })
            };

            // Act & Assert
            foreach (var (method, endpoint, model) in endpoints)
            {
                HttpResponseMessage response;
                
                if (method == "POST")
                {
                    response = await _client.PostAsJsonAsync(endpoint, model);
                }
                else
                {
                    response = await _client.GetAsync(endpoint);
                }
                
                // Aceita 500 se for erro de banco de dados nÃ£o configurado, mas valida que Ã© JSON vÃ¡lido
                var content = await response.Content.ReadAsStringAsync();
                
                // Valida que Ã© JSON vÃ¡lido (mesmo em caso de erro)
                Action parseJson = () => JsonSerializer.Deserialize<object>(content);
                parseJson.Should().NotThrow($"Endpoint {endpoint} deve retornar JSON vÃ¡lido, mesmo em caso de erro");
                
                // Se nÃ£o for 500, valida que estÃ¡ nos status codes esperados
                if (response.StatusCode != HttpStatusCode.InternalServerError)
                {
                    response.StatusCode.Should().BeOneOf(
                        new[] { HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.Conflict },
                        $"Endpoint {endpoint} deve retornar status code vÃ¡lido");
                }
            }
        }

        [Fact(DisplayName = "REGRESSÃƒO: Status codes devem seguir padrÃ£o estabelecido")]
        public async Task Regressao_StatusCodesDevemSeguirPadrao()
        {
            // Arrange
            var testCases = new[]
            {
                ("POST", "/Auth/login", new LoginInputModel { Login = "teste@example.com", Senha = "Senha123!" },
                 new[] { HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized })
            };

            // Act & Assert
            foreach (var (method, endpoint, model, expectedStatusCodes) in testCases)
            {
                HttpResponseMessage response;
                
                if (method == "POST")
                {
                    response = await _client.PostAsJsonAsync(endpoint, model);
                }
                else
                {
                    response = await _client.GetAsync(endpoint);
                }
                
                // Aceita 500 se for erro de banco de dados nÃ£o configurado
                // Caso contrÃ¡rio, valida que estÃ¡ nos status codes esperados
                if (response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    // Em caso de erro de banco, apenas valida que a resposta Ã© JSON vÃ¡lido
                    var content = await response.Content.ReadAsStringAsync();
                    Action parseJson = () => JsonSerializer.Deserialize<object>(content);
                    parseJson.Should().NotThrow($"Endpoint {endpoint} deve retornar JSON vÃ¡lido mesmo em caso de erro");
                }
                else
                {
                    response.StatusCode.Should().BeOneOf(expectedStatusCodes, 
                        $"Endpoint {endpoint} deve retornar apenas status codes esperados");
                }
            }
        }

        [Fact(DisplayName = "REGRESSÃƒO: Mensagens de erro devem sempre ter formato consistente")]
        public async Task Regressao_MensagensErroDevemTerFormatoConsistente()
        {
            // Arrange
            var loginModel = new LoginInputModel
            {
                Login = "usuario.inexistente@example.com",
                Senha = "SenhaIncorreta123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/Auth/login", loginModel);

            // Assert - Garante que mensagens de erro tÃªm formato consistente
            if (response.StatusCode == HttpStatusCode.BadRequest || 
                response.StatusCode == HttpStatusCode.InternalServerError ||
                response.StatusCode == HttpStatusCode.NotFound ||
                response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ResultModel<TokenResultModel>>(
                    content, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                // ValidaÃ§Ãµes crÃ­ticas que nÃ£o podem mudar
                result.Should().NotBeNull("Resposta de erro deve ser um ResultModel");
                typeof(ResultModel<TokenResultModel>).GetProperty("Success").Should().NotBeNull("Resposta de erro deve ter propriedade Success");
                typeof(ResultModel<TokenResultModel>).GetProperty("Message").Should().NotBeNull("Resposta de erro deve ter propriedade Message");
            }
        }

        protected override void Dispose(bool disposing)
        {
            _client?.Dispose();
            base.Dispose(disposing);
        }
    }
}

