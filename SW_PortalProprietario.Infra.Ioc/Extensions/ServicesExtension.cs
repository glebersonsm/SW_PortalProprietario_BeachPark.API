using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Interfaces.ReservasApi;
using SW_PortalProprietario.Application.Services.Core;
using SW_PortalProprietario.Application.Services.Core.AutomaticCommunications;
using SW_PortalProprietario.Application.Services.Core.AutomaticCommunications.GenerationServices;
using SW_PortalProprietario.Application.Services.Core.AutomaticCommunications.Handlers.AvisoCheckin;
using SW_PortalProprietario.Application.Services.Core.AutomaticCommunications.Handlers.IncentivoAgendamento;
using SW_PortalProprietario.Application.Services.Core.AutomaticCommunications.Handlers.Voucher;
using SW_PortalProprietario.Application.Services.Core.AutomaticCommunications.Proccessing.AvisoCheckin;
using SW_PortalProprietario.Application.Services.Core.AutomaticCommunications.Proccessing.IncentivoAgendamento;
using SW_PortalProprietario.Application.Services.Core.AutomaticCommunications.Proccessing.Voucher;
using SW_PortalProprietario.Application.Services.Core.AutomaticCommunications.Simulation.AvisoCheckin;
using SW_PortalProprietario.Application.Services.Core.AutomaticCommunications.Simulation.EnvioVoucher;
using SW_PortalProprietario.Application.Services.Core.AutomaticCommunications.Simulation.IncentivoAgendamento;
using SW_PortalProprietario.Application.Services.Core.Auxiliar;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Application.Services.Core.Interfaces.Communication;
using SW_PortalProprietario.Application.Services.Core.Pessoa;
using SW_PortalProprietario.Application.Services.Providers.Esolution;
using SW_PortalProprietario.Application.Services.Providers.Interfaces;
using SW_PortalProprietario.Application.Services.ReservasApi;
using SW_PortalProprietario.Infra.Data.Repositories.Core;
using SW_PortalProprietario.Infra.Ioc.Broker;
using SW_PortalProprietario.Infra.Ioc.Communication;
using SW_Utils.Historicos;

namespace SW_PortalProprietario.Infra.Ioc.Extensions
{
    public static class ServicesExtension
    {
        public static IServiceCollection AddSystemServices(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services, nameof(services));
            services.AddScoped<IUnitOfWorkNHDefault, UnitOfWorkNHDefault>();
            services.TryAddScoped<IRepositoryNH, RepositoryNH>();
            services.TryAddScoped<IAuthenticatedBaseService, AuthenticatedBaseService>();
            services.TryAddScoped<IAuthService, AuthService>();
            services.TryAddScoped<IUserService, UserService>();
            services.TryAddScoped<IFrameworkService, FrameworkService>();
            services.TryAddScoped<ICountryService, CountryService>();
            services.TryAddScoped<IStateService, StateService>();
            services.TryAddScoped<ICityService, CityService>();
            services.TryAddScoped<ITipoTelefoneService, TipoTelefoneService>();
            services.TryAddScoped<ITipoEnderecoService, TipoEnderecoService>();
            services.TryAddScoped<ITipoDocumentoPessoaService, TipoDocumentoPessoaService>();
            services.TryAddScoped<IPessoaTelefoneService, PessoaTelefoneService>();
            services.TryAddScoped<IPessoaEnderecoService, PessoaEnderecoService>();
            services.TryAddScoped<IPessoaDocumentoService, PessoaDocumentoService>();
            services.TryAddScoped<IPessoaService, PessoaService>();
            services.TryAddScoped<IServiceBase, ServiceBase>();
            services.TryAddScoped<IDocumentGroupService, DocumentGroupService>();
            services.TryAddScoped<IDocumentService, DocumentService>();
            services.TryAddScoped<IRegraPaxFreeService, RegraPaxFreeService>();
            services.TryAddScoped<IFaqGroupService, FaqGroupService>();
            services.TryAddScoped<IFaqService, FaqService>();
            services.TryAddScoped<ITagsService, TagsService>();
            services.TryAddScoped<IEmailService, EmailService>();
            services.TryAddScoped<IEmailHostedService, EmailHostedService>();
            services.AddHttpClient<ISmsProvider, BeachParkSmsService>();
            services.AddHttpClient();
            services.TryAddSingleton<ISmtpSettingsProvider, ParametroSistemaSmtpSettingsProvider>();
            services.TryAddScoped<IBroker, NovaXSPaymentBroker>();
            services.TryAddScoped<IFinanceiroTransacaoService, FinanceiroTransacaoService>();
            services.TryAddScoped<IFinanceiroTransacaoUsuarioService, FinanceiroUsuarioTransacaoService>();
            services.TryAddScoped<IImageGroupService, ImageGroupService>();
            services.TryAddScoped<IImageGroupImageService, ImageGroupImageService>();
            services.TryAddScoped<IGrupoImagemHomeService, GrupoImagemHomeService>();
            services.TryAddScoped<IImagemGrupoImagemHomeService, ImagemGrupoImagemHomeService>();
            services.TryAddScoped<ICertidaoFinanceiraService, CertidaoFinanceiraService>();
            services.TryAddScoped<IHistoricosCertidoes, HistoricosCertidoes>();
            services.TryAddScoped<IHtmlTemplateService, HtmlTemplateService>();
            services.TryAddScoped<IScriptService, ScriptService>();
            services.TryAddScoped<ITokenBodyService, JwtTokenService>();
            services.TryAddScoped<IParametroSistemaService, ParametroSistemaService>();
            services.TryAddScoped<IRabbitMQQueueService, RabbitMQQueueService>();
            services.TryAddScoped<IReservaAgendamentoService, ReservaAgendamentoService>();
            services.TryAddScoped<IDocumentTemplateService, DocumentTemplateService>();
            services.TryAddScoped<IVoucherReservaService, VoucherReservaService>();
            services.TryAddScoped<Application.Services.Core.Interfaces.IIncentivoParaAgendamentoDocumentService, Application.Services.Core.IncentivoParaAgendamentoService>();
            services.TryAddScoped<IAutomaticCommunicationConfigService, AutomaticCommunicationConfigService>();
            services.TryAddScoped<IVhfConfigService, VhfConfigService>();
            services.TryAddScoped<IRegraIntercambioService, RegraIntercambioService>();
            services.TryAddScoped<IAutomaticVoucherService, AutomaticVoucherService>();

            // Registrar Communication Handlers e serviços auxiliares
            //Simuladores
            services.TryAddScoped<VoucherSimulationService>();
            services.TryAddScoped<AvisoCheckinSimulationService>();
            services.TryAddScoped<IncentivoAgendamentoSimulationService>();

            //Serviços de geração (GenerationServices)
            services.TryAddScoped<VoucherGenerationService>();
            services.TryAddScoped<AvisoCheckinGenerationService>();
            services.TryAddScoped<IncentivoAgendamentoGenerationService>();

            //Serviços de processamento em massa (ProcessingServices)
            services.TryAddScoped<VoucherProcessingService>();
            services.TryAddScoped<AvisoCheckinProcessingService>();
            services.TryAddScoped<IncentivoAgendamentoProcessingService>();

            services.AddScoped<ICommunicationHandler, VoucherReservaCommunicationHandler>();
            services.AddScoped<ICommunicationHandler, AvisoReservaCheckinProximoCommunicationHandler>();
            services.AddScoped<ICommunicationHandler, IncentivoParaAgendamentoHandler>();

            services.TryAddScoped<ICommunicationHandlerFactory, CommunicationHandlerFactory>();

            services.TryAddScoped<IAuditService, AuditService>();
            services.TryAddScoped<IAuditCacheService, AuditCacheService>();

            return services;
        }

    }
}
