using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Models.SystemModels
{
    public class ParametroSistemaViewModel
    {
        public int? Id { get; set; }
        public string? SiteParaReserva { get; set; }
        public int? EmpresaId { get; set; }
        public EnumSimNao? AgruparCertidaoPorCliente { get; set; }
        public EnumSimNao? EmitirCertidaoPorUnidCliente { get; set; }
        public string? ExibirFinanceirosDasEmpresaIds { get; set; }
        public EnumSimNao? HabilitarBaixarBoleto { get; set; }
        public EnumSimNao? HabilitarPagamentosOnLine { get; set; }
        public EnumSimNao? HabilitarPagamentoEmPix { get; set; }
        public EnumSimNao? HabilitarPagamentoEmCartao { get; set; }
        public EnumSimNao? ExibirContasVencidas { get; set; }
        public int? QtdeMaximaDiasContasAVencer { get; set; }
        public EnumSimNao? PermitirUsuarioAlterarSeuEmail { get; set; }
        public EnumSimNao? PermitirUsuarioAlterarSeuDoc { get; set; }
        public EnumSimNao? IntegradoComMultiPropriedade { get; set; }
        public EnumSimNao? IntegradoComTimeSharing { get; set; }
        public string? ServerAddress { get; set; }
        public string? NomeCondominio { get; set; }
        public string? CnpjCondominio { get; set; }
        public string? EnderecoCondominio { get; set; }

        public string? NomeAdministradoraCondominio { get; set; }
        public string? CnpjAdministradoraCondominio { get; set; }
        public string? EnderecoAdministradoraCondominio { get; set; }
        public int? EmpreendimentoId { get; set; }
        public int? PontosRci { get; set; }
        public List<PessoaSistemaXProviderModel>? PessoaSistemaXProviders { get; set; }
        
        #region ConfiguraÃ§Ãµes de Reserva - Campos ObrigatÃ³rios para HÃ³spedes Convidados
        public EnumSimNao? ExigeEnderecoHospedeConvidado { get; set; }
        public EnumSimNao? ExigeTelefoneHospedeConvidado { get; set; }
        public EnumSimNao? ExigeDocumentoHospedeConvidado { get; set; }
        #endregion

        #region ConfiguraÃ§Ãµes de Reserva RCI
        public EnumSimNao? PermiteReservaRciApenasClientesComContratoRci { get; set; }
        #endregion

        #region AutenticaÃ§Ã£o em duas etapas (2FA)
        public EnumSimNao? Habilitar2FAPorEmail { get; set; }
        public EnumSimNao? Habilitar2FAPorSms { get; set; }
        public EnumSimNao? Habilitar2FAParaCliente { get; set; }
        public EnumSimNao? Habilitar2FAParaAdministrador { get; set; }
        /// <summary> URL do endpoint de envio de SMS para token 2FA. </summary>
        public string? EndpointEnvioSms2FA { get; set; }
        #endregion

        #region ConfiguraÃ§Ãµes de envio de e-mail (SMTP)
        public string? SmtpHost { get; set; }
        public int? SmtpPort { get; set; }
        public EnumSimNao? SmtpUseSsl { get; set; }
        public string? SmtpIamUser { get; set; }
        public string? SmtpUser { get; set; }
        public string? SmtpPass { get; set; }
        public string? SmtpFromName { get; set; }
        public EnumTipoEnvioEmail? TipoEnvioEmail { get; set; }
        public string? EmailTrackingBaseUrl { get; set; }
        #endregion

        #region ConfiguraÃ§Ãµes de ImportaÃ§Ã£o de UsuÃ¡rios/Clientes do Legado
        public EnumSimNao? CriarUsuariosLegado { get; set; }
        public EnumSimNao? CriarUsuariosClientesLegado { get; set; }
        #endregion

    }
}
