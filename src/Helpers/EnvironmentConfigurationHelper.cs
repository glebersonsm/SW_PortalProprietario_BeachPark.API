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

        /// <summary>
        /// Obtém um valor de configuração priorizando variável de ambiente (.env) sobre appsettings.json
        /// </summary>
        public static string? GetConfigValue(IConfiguration configuration, string configKey, string? envKey = null, string? defaultValue = null)
        {
            // 1. Tentar obter da variável de ambiente (se envKey foi fornecida)
            if (!string.IsNullOrWhiteSpace(envKey))
            {
                var envValue = Environment.GetEnvironmentVariable(envKey);
                if (!string.IsNullOrWhiteSpace(envValue))
                {
                    return envValue;
                }
            }

            // 2. Tentar obter do appsettings.json
            var configValue = configuration[configKey];
            if (!string.IsNullOrWhiteSpace(configValue))
            {
                return configValue;
            }

            // 3. Retornar valor padrão
            return defaultValue;
        }

        /// <summary>
        /// Obtém um valor de configuração tipado priorizando variável de ambiente (.env) sobre appsettings.json
        /// </summary>
        public static T? GetConfigValue<T>(IConfiguration configuration, string configKey, string? envKey = null, T? defaultValue = default)
        {
            var stringValue = GetConfigValue(configuration, configKey, envKey, defaultValue?.ToString());
            
            if (string.IsNullOrWhiteSpace(stringValue))
                return defaultValue;

            try
            {
                return (T)Convert.ChangeType(stringValue, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
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
            OverrideIfNotEmpty(configuration, "ProgramId", "PROGRAM_ID");

            // RabbitMQ
            OverrideIfNotEmpty(configuration, "RabbitMqConnectionHost", "RABBITMQ_HOST");
            OverrideIfNotEmpty(configuration, "RabbitMqConnectionPort", "RABBITMQ_PORT");
            OverrideIfNotEmpty(configuration, "RabbitMqConnectionUser", "RABBITMQ_USER");
            OverrideIfNotEmpty(configuration, "RabbitMqConnectionPass", "RABBITMQ_PASS");
            OverrideIfNotEmpty(configuration, "RabbitMqFilaDeAuditoriaNome", "RABBITMQ_FILA_AUDITORIA");
            OverrideIfNotEmpty(configuration, "RabbitMqFilaDeLogNome", "RABBITMQ_FILA_LOG");
            OverrideIfNotEmpty(configuration, "RabbitMqFilaDeEmailNome", "RABBITMQ_FILA_EMAIL");

            // CM API
            OverrideIfNotEmpty(configuration, "CMUserId", "CM_USER_ID");
            OverrideIfNotEmpty(configuration, "ReservasCMApiConfig:BaseUrl", "RESERVAS_CM_API_BASE_URL");
            OverrideIfNotEmpty(configuration, "ReservasCMApiConfig:IdPromotorRci", "RESERVAS_CM_API_ID_PROMOTOR_RCI");
            OverrideIfNotEmpty(configuration, "ReservasCMApiConfig:ReservanteRci", "RESERVAS_CM_API_RESERVANTE_RCI");
            OverrideIfNotEmpty(configuration, "ReservasCMApiConfig:TelReservanteRci", "RESERVAS_CM_API_TEL_RESERVANTE_RCI");
            OverrideIfNotEmpty(configuration, "ReservasCMApiConfig:IdHospedeBulkBank", "RESERVAS_CM_API_ID_HOSPEDE_BULK_BANK");

            // SMTP: configuração movida para o sistema (ParametroSistema). Não sobrescrever por .env.

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
            OverrideIfNotEmpty(configuration, "TipoAmbiente", "TIPO_AMBIENTE");
            OverrideIfNotEmpty(configuration, "EnviarEmailApenasParaDestinatariosPermitidos", "ENVIAR_EMAIL_APENAS_PARA_DESTINATARIOS_PERMITIDOS");
            OverrideIfNotEmpty(configuration, "DestinatarioEmailPermitido", "DESTINATARIO_EMAIL_PERMITIDO");
            OverrideIfNotEmpty(configuration, "EnviarSmsApenasParaNumeroPermitido", "ENVIAR_SMS_APENAS_PARA_NUMERO_PERMITIDO");
            OverrideIfNotEmpty(configuration, "NumeroSmsPermitido", "NUMERO_SMS_PERMITIDO");
            OverrideIfNotEmpty(configuration, "ConnectionStrings:ForceRollback", "FORCE_ROLLBACK");

            // CORS
            OverrideIfNotEmpty(configuration, "OrigensPermitidas", "ORIGENS_PERMITIDAS");

            // Other
            OverrideIfNotEmpty(configuration, "SoFaltaEuUrl", "SO_FALTA_EU_URL");
            OverrideIfNotEmpty(configuration, "UsuarioSistemaId", "USUARIO_SISTEMA_ID");
            OverrideIfNotEmpty(configuration, "EmpresaSwPortalId", "EMPRESA_SW_PORTAL_ID");
            OverrideIfNotEmpty(configuration, "EmpresaCMId", "EMPRESA_CM_ID");
            OverrideIfNotEmpty(configuration, "ControleDeUsuarioViaSFE", "CONTROLE_USUARIO_SFE");
            OverrideIfNotEmpty(configuration, "ControleDeUsuarioViaAccessCenter", "CONTROLE_USUARIO_ACCESS_CENTER");
            OverrideIfNotEmpty(configuration, "IntegradoCom", "INTEGRADO_COM");
            OverrideIfNotEmpty(configuration, "TagGeralId", "TAG_GERAL_ID");
            OverrideIfNotEmpty(configuration, "TagTropicalId", "TAG_TROPICAL_ID");
            OverrideIfNotEmpty(configuration, "TagHomesId", "TAG_HOMES_ID");
            OverrideIfNotEmpty(configuration, "BloquearCriacaoAdmForaDebugMode", "BLOQUEAR_CRIACAO_ADM_FORA_DEBUG");
            OverrideIfNotEmpty(configuration, "AdmGroupDefaultName", "ADM_GROUP_DEFAULT_NAME");
            // Audit Log
            OverrideIfNotEmpty(configuration, "AuditLog:ConsumerConcurrency", "AUDIT_LOG_CONSUMER_CONCURRENCY");
            OverrideIfNotEmpty(configuration, "AuditLog:RetryAttempts", "AUDIT_LOG_RETRY_ATTEMPTS");
            OverrideIfNotEmpty(configuration, "AuditLog:RetryDelaySeconds", "AUDIT_LOG_RETRY_DELAY_SECONDS");

            // Processing Queues
            OverrideIfNotEmpty(configuration, "SendOperationsToProcessingLogQueue", "SEND_OPERATIONS_TO_LOG_QUEUE");
            OverrideIfNotEmpty(configuration, "SaveLogOperationsFromProcessingLogQueue", "SAVE_LOG_FROM_QUEUE");
            OverrideIfNotEmpty(configuration, "SendEmailFromProcessingQueue", "SEND_EMAIL_FROM_QUEUE");
            OverrideIfNotEmpty(configuration, "AutomaticCommunicationEmailEnabled", "AUTOMATIC_EMAIL_ENABLED");

            // Wait Times
            OverrideIfNotEmpty(configuration, "TimeWaitInMinutesSendOperationsToPorcessingLogQueue", "WAIT_SEND_LOG_QUEUE");
            OverrideIfNotEmpty(configuration, "TimeWaitInMinutesSaveOperationsFromPorcessingLogQueueConsumer", "WAIT_SAVE_LOG_QUEUE");
            OverrideIfNotEmpty(configuration, "TimeWaitInMinutesSandEmailFromPorcessingQueueConsumer", "WAIT_SEND_EMAIL_QUEUE");
            OverrideIfNotEmpty(configuration, "TimeWaitInSecondsSearchPixResult", "WAIT_SEARCH_PIX");
            OverrideIfNotEmpty(configuration, "TimeWaitInSecondsFinalizeCartaoResult", "WAIT_FINALIZE_CARTAO");

            // Specific Certidões Config
            OverrideIfNotEmpty(configuration, "CertidoesConfig:PositivaConfigPorUnidade", "CERTIDOES_POSITIVA_UNIDADE");
            OverrideIfNotEmpty(configuration, "CertidoesConfig:NegativaConfigPorUnidade", "CERTIDOES_NEGATIVA_UNIDADE");
            OverrideIfNotEmpty(configuration, "CertidoesConfig:PositivaConfigPorCliente", "CERTIDOES_POSITIVA_CLIENTE");
            OverrideIfNotEmpty(configuration, "CertidoesConfig:NegativaConfigPorCliente", "CERTIDOES_NEGATIVA_CLIENTE");
            OverrideIfNotEmpty(configuration, "CertidoesConfig:ContratoSCPPorEmpresa", "CERTIDOES_CONTRATO_SCP_EMPRESA");
            OverrideIfNotEmpty(configuration, "CertidoesConfig:ContratoSCPEspanhol", "CERTIDOES_CONTRATO_SCP_ESPANHOL");
            OverrideIfNotEmpty(configuration, "CertidoesConfig:AgruparCertidaoPorCliente", "CERTIDOES_AGRUPAR_CLIENTE");
            OverrideIfNotEmpty(configuration, "CertidoesConfig:PathValidacaoProtocolo", "CERTIDOES_PATH_VALIDACAO");

            // Broker & Others
            OverrideIfNotEmpty(configuration, "UseBrokerType", "USE_BROKER_TYPE");
            OverrideIfNotEmpty(configuration, "PodeInformarDadosDePixParaRecebimentoSCP", "PODE_INFORMAR_PIX_SCP");
            OverrideIfNotEmpty(configuration, "TimeSharingAtivado", "TIME_SHARING_ATIVADO");
            OverrideIfNotEmpty(configuration, "MultipropriedadeAtivada", "MULTIPROPRIEDADE_ATIVADA");
            OverrideIfNotEmpty(configuration, "UpdateDataBase", "UPDATE_DATABASE");
            OverrideIfNotEmpty(configuration, "UpdateFramework", "UPDATE_FRAMEWORK");
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
