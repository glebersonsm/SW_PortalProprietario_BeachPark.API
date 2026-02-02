using System;

namespace SW_PortalCliente_BeachPark.API.Helpers
{
    public static class EnvironmentConfigurationHelper
    {
        public static string GetConnectionString(string name)
        {
            return name switch
            {
                "CmConnection" => Environment.GetEnvironmentVariable("CM_CONNECTION") ?? string.Empty,
                "EsolAccessCenterConnection" => Environment.GetEnvironmentVariable("ESOL_ACCESS_CENTER_CONNECTION") ?? string.Empty,
                "EsolPortalConnection" => Environment.GetEnvironmentVariable("ESOL_PORTAL_CONNECTION") ?? string.Empty,
                "DefaultConnection" => Environment.GetEnvironmentVariable("DEFAULT_CONNECTION") ?? string.Empty,
                "RedisConnection" => Environment.GetEnvironmentVariable("REDIS_CONNECTION") ?? string.Empty,
                _ => string.Empty
            };
        }

        public static void OverrideConfigurationWithEnvironmentVariables(IConfiguration configuration)
        {
            // JWT
            OverrideIfNotEmpty(configuration, "Jwt:Key", "JWT_KEY");
            OverrideIfNotEmpty(configuration, "Jwt:Issuer", "JWT_ISSUER");
            OverrideIfNotEmpty(configuration, "Jwt:Audience", "JWT_AUDIENCE");

            // TSE
            OverrideIfNotEmpty(configuration, "TSEConfig:ApiAddress", "TSE_API_ADDRESS");
            OverrideIfNotEmpty(configuration, "TSEConfig:LoginPath", "TSE_LOGIN_PATH");
            OverrideIfNotEmpty(configuration, "TSEConfig:User", "TSE_USER");
            OverrideIfNotEmpty(configuration, "TSEConfig:Pass", "TSE_PASS");
            OverrideIfNotEmpty(configuration, "TSEConfig:Pass1", "TSE_PASS");

            // Connection Strings
            OverrideIfNotEmpty(configuration, "ConnectionStrings:CmConnection", "CM_CONNECTION");
            OverrideIfNotEmpty(configuration, "ConnectionStrings:EsolAccessCenterConnection", "ESOL_ACCESS_CENTER_CONNECTION");
            OverrideIfNotEmpty(configuration, "ConnectionStrings:EsolPortalConnection", "ESOL_PORTAL_CONNECTION");
            OverrideIfNotEmpty(configuration, "ConnectionStrings:DefaultConnection", "DEFAULT_CONNECTION");
            OverrideIfNotEmpty(configuration, "ConnectionStrings:RedisConnection", "REDIS_CONNECTION");

            // Redis
            OverrideIfNotEmpty(configuration, "Redis:Password", "REDIS_PASSWORD");
            OverrideIfNotEmpty(configuration, "Redis:Hosts:0:Host", "REDIS_HOST");
            OverrideIfNotEmpty(configuration, "Redis:Hosts:0:Port", "REDIS_PORT");
            OverrideIfNotEmpty(configuration, "Redis:Database", "REDIS_DATABASE");

            // RabbitMQ
            OverrideIfNotEmpty(configuration, "RabbitMqConnectionHost", "RABBITMQ_HOST");
            OverrideIfNotEmpty(configuration, "RabbitMqConnectionPort", "RABBITMQ_PORT");
            OverrideIfNotEmpty(configuration, "RabbitMqConnectionUser", "RABBITMQ_USER");
            OverrideIfNotEmpty(configuration, "RabbitMqConnectionPass", "RABBITMQ_PASS");

            // CM API
            OverrideIfNotEmpty(configuration, "CMUserId", "CM_USER_ID");
            OverrideIfNotEmpty(configuration, "ReservasCMApiConfig:BaseUrl", "RESERVAS_CM_API_BASE_URL");
            OverrideIfNotEmpty(configuration, "ReservasCMApiConfig:IdPromotorRci", "RESERVAS_CM_API_ID_PROMOTOR_RCI");
            OverrideIfNotEmpty(configuration, "ReservasCMApiConfig:ReservanteRci", "RESERVAS_CM_API_RESERVANTE_RCI");
            OverrideIfNotEmpty(configuration, "ReservasCMApiConfig:TelReservanteRci", "RESERVAS_CM_API_TEL_RESERVANTE_RCI");
            OverrideIfNotEmpty(configuration, "ReservasCMApiConfig:IdHospedeBulkBank", "RESERVAS_CM_API_ID_HOSPEDE_BULK_BANK");

            // SMTP
            OverrideIfNotEmpty(configuration, "SmtpHost", "SMTP_HOST");
            OverrideIfNotEmpty(configuration, "SmtpPort", "SMTP_PORT");
            OverrideIfNotEmpty(configuration, "SmtpUseSsl", "SMTP_USE_SSL");
            OverrideIfNotEmpty(configuration, "SmtpUser", "SMTP_USER");
            OverrideIfNotEmpty(configuration, "SmtpPass", "SMTP_PASS");
            OverrideIfNotEmpty(configuration, "SmtpFromName", "SMTP_FROM_NAME");

            // Paths
            OverrideIfNotEmpty(configuration, "WwwRootPath", "WWWROOT_PATH");
            OverrideIfNotEmpty(configuration, "WwwRootImagePath", "WWWROOT_IMAGE_PATH");
            OverrideIfNotEmpty(configuration, "WwwRootGrupoImagePath", "WWWROOT_GRUPO_IMAGE_PATH");
            OverrideIfNotEmpty(configuration, "NewDocumentsFilePath", "NEW_DOCUMENTS_FILE_PATH");
            OverrideIfNotEmpty(configuration, "PathGeracaoPdfComunicacoesGeraia", "PATH_GERACAO_PDF_COMUNICACOES_GERAIS");
            OverrideIfNotEmpty(configuration, "PathGeracaoBoletos", "PATH_GERACAO_BOLETOS");
            OverrideIfNotEmpty(configuration, "CertidoesConfig:ModelosCertidoesPath", "CERTIDOES_MODELOS_PATH");
            OverrideIfNotEmpty(configuration, "CertidoesConfig:GeracaoPdfPath", "CERTIDOES_GERACAO_PDF_PATH");
            OverrideIfNotEmpty(configuration, "CertidoesConfig:GeracaoPdfContratoPath", "CERTIDOES_GERACAO_PDF_CONTRATO_PATH");

            // Development/Testing
            OverrideIfNotEmpty(configuration, "IgnorarValidacaoLogin", "IGNORAR_VALIDACAO_LOGIN");
            OverrideIfNotEmpty(configuration, "UsarSenhaPadraoAmbienteHomologacao", "USAR_SENHA_PADRAO_AMBIENTE_HOMOLOGACAO");
            OverrideIfNotEmpty(configuration, "EnviarEmailApenasParaDestinatariosPermitidos", "ENVIAR_EMAIL_APENAS_PARA_DESTINATARIOS_PERMITIDOS");
            OverrideIfNotEmpty(configuration, "DestinatarioEmailPermitido", "DESTINATARIO_EMAIL_PERMITIDO");

            // CORS
            OverrideIfNotEmpty(configuration, "OrigensPermitidas", "ORIGENS_PERMITIDAS");

            // Other
            OverrideIfNotEmpty(configuration, "SoFaltaEuUrl", "SO_FALTA_EU_URL");
            OverrideIfNotEmpty(configuration, "UsuarioSistemaId", "USUARIO_SISTEMA_ID");
        }

        private static void OverrideIfNotEmpty(IConfiguration configuration, string configKey, string envKey)
        {
            var envValue = Environment.GetEnvironmentVariable(envKey);
            if (!string.IsNullOrWhiteSpace(envValue))
            {
                configuration[configKey] = envValue;
            }
        }
    }
}