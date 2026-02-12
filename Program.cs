using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog.Extensions.Logging;
using SW_PortalCliente_BeachPark.API.src.Filters;
using SW_PortalProprietario.Application.Models.Financeiro;
using SW_PortalProprietario.Infra.Ioc.Extensions;
using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using DotNetEnv;
using SW_PortalCliente_BeachPark.API.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Carrega as variáveis de ambiente do arquivo .env
// Tenta primeiro no diretório base da aplicação (bin/Debug ou bin/Release)
var envPath = Path.Combine(AppContext.BaseDirectory, ".env");

// Se não encontrar, tenta na raiz do projeto (útil durante desenvolvimento)
if (!File.Exists(envPath))
{
    var projectRoot = Directory.GetCurrentDirectory();
    envPath = Path.Combine(projectRoot, ".env");
}

if (File.Exists(envPath))
{
    Console.WriteLine($"Arquivo .env carregado de: {envPath}");
    Env.Load(envPath);
}
else
{
    Console.WriteLine($"AVISO: Arquivo .env não encontrado.");
    Console.WriteLine($"  - Tentado em: {Path.Combine(AppContext.BaseDirectory, ".env")}");
    Console.WriteLine($"  - Tentado em: {Path.Combine(Directory.GetCurrentDirectory(), ".env")}");
    Console.WriteLine($"  - Certifique-se de que o arquivo .env existe e está configurado para ser copiado no build.");
}

// Carrega as configurações dos arquivos JSON
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

// Carrega o arquivo de configuração específico (BeachParkConfigurations.json ou outro)
var includeFile = builder.Configuration["IncludeFile"];
if (!string.IsNullOrEmpty(includeFile))
{
    builder.Configuration.AddJsonFile(includeFile, optional: true, reloadOnChange: true);
}

// Sobrescreve as configurações com as variáveis de ambiente do .env
EnvironmentConfigurationHelper.OverrideConfigurationWithEnvironmentVariables(builder.Configuration);

builder.Services.AddRedisCache(builder.Configuration);

var configBrokerNameToUse = builder.Configuration.GetValue("UseBrokerType", "BrokerNaoConfigurado") ?? "BrokerNaoConfigurado";

builder.Services.Configure<BrokerModel>(builder.Configuration.GetSection(configBrokerNameToUse));

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SW_Portal_Cliente.Main",
        Version = "v1",
        Contact = new OpenApiContact
        {
            Name = "SW Soluções Integradas Ltda",
            Email = "contato@swsolucoes.inf.br",
            Url = new Uri("https://www.swsolucoes.inf.br")
        }
    });

    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme()
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    c.OperationFilter<SecurityRequirementsOperationFilter>();
});

builder.Services.AddSystemServices();
builder.Services.ConfigureRabbitMQ();
builder.Services.AddObjectMapping();
builder.Services.AddScoped<LogRequestFilter>();

builder.Services.AddNHbernate(builder.Configuration);

builder.Services.RegisterHostedServices(builder.Configuration);

builder.Services.RegisterProviders(builder.Configuration);

builder.Services.AddControllers(options =>
            options.Filters.Add<LogRequestFilter>())
    .AddJsonOptions(
        opt =>
        {
            opt.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            opt.JsonSerializerOptions.WriteIndented = true;
            opt.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            opt.JsonSerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        }
    );

builder.Services.AddCors(options =>
{
    var origens = builder.Configuration.GetValue<string>("OrigensPermitidas");

    options.AddPolicy("AllowAll", policy =>
    {
        if (!string.IsNullOrEmpty(origens))
        {
            policy.WithOrigins(origens.Split('|').ToArray())
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
        else
        {
            policy.SetIsOriginAllowed(origin => true)
             .AllowAnyHeader()
             .AllowAnyMethod()
             .AllowCredentials();
        }
    });
});

builder.Services.AddEndpointsApiExplorer();


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {

        ValidateActor = false,
        ValidateAudience = false,
        ValidateIssuer = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration.GetValue<string>("Jwt:Issuer")!,
        ValidAudience = builder.Configuration.GetValue<string>("Jwt:Audience")!,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration.GetValue<string>("Jwt:Key")!))
    };

});

builder.Logging.AddNLog(builder.Configuration.GetSection("Logging"));


builder.Services.AddHttpContextAccessor();

// Registrar AuditHelper
builder.Services.AddScoped<SW_PortalProprietario.Infra.Data.Audit.AuditHelper>();

var app = builder.Build();
app.UseCors("AllowAll");

// Garante charset UTF-8 em respostas JSON para acentuação correta
app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        var contentType = context.Response.Headers.ContentType.ToString();
        if (contentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase)
            && !contentType.Contains("charset=", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.Headers.ContentType = "application/json; charset=utf-8";
        }
        return Task.CompletedTask;
    });
    await next();
});

// Adicionar middleware de contexto de auditoria
app.UseMiddleware<SW_PortalProprietario.Infra.Data.Middleware.AuditContextMiddleware>();


app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SW_Solucoes_Portal_Proprietario_MVC_Api_Principal v1");
    c.EnableTryItOutByDefault();
    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
    c.DisplayRequestDuration();
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


app.Run();

// Torna a classe Program acessível para testes de integração
namespace SW_PortalCliente_BeachPark.API
{
    public partial class Program { }
}