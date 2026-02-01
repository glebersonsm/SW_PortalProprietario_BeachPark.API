using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog.Extensions.Logging;
using SW_PortalProprietario.API.src.Filters;
using SW_PortalProprietario.Application.Models.Financeiro;
using SW_PortalProprietario.Infra.Ioc.Extensions;
using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

var baseDirectory = Directory.GetCurrentDirectory();

var configuration = builder.Configuration
    .SetBasePath(baseDirectory)
    .AddJsonFile(Debugger.IsAttached ? "appsettings.Development.json" : "appsettings.json", optional: false, reloadOnChange: true);

var additionalFile = builder.Configuration.GetValue<string>("IncludeFile");
if (!string.IsNullOrEmpty(additionalFile))
{
    builder.Configuration.AddJsonFile(additionalFile, optional: false, reloadOnChange: true);
}

builder.Services.AddRedisCache(builder.Configuration);

var configBrokerNameToUse = builder.Configuration.GetValue<string>("UseBrokerType", "BrokerNaoConfigurado") ?? "BrokerNaoConfigurado";

builder.Services.Configure<BrokerModel>(builder.Configuration.GetSection(configBrokerNameToUse));

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "SW_Portal_Proprietario.Main",
        Version = "v1",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
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
namespace SW_PortalProprietario.API
{
    public partial class Program { }
}