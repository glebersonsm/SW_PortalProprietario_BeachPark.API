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
        public string? ImagemHomeUrl1 { get; set; }
        public string? ImagemHomeUrl2 { get; set; }
        public string? ImagemHomeUrl3 { get; set; }
        public string? ImagemHomeUrl4 { get; set; }
        public string? ImagemHomeUrl5 { get; set; }
        public string? ImagemHomeUrl6 { get; set; }
        public string? ImagemHomeUrl7 { get; set; }
        public string? ImagemHomeUrl8 { get; set; }
        public string? ImagemHomeUrl9 { get; set; }
        public string? ImagemHomeUrl10 { get; set; }
        public string? ImagemHomeUrl11 { get; set; }
        public string? ImagemHomeUrl12 { get; set; }
        public string? ImagemHomeUrl13 { get; set; }
        public string? ImagemHomeUrl14 { get; set; }
        public string? ImagemHomeUrl15 { get; set; }
        public string? ImagemHomeUrl16 { get; set; }
        public string? ImagemHomeUrl17 { get; set; }
        public string? ImagemHomeUrl18 { get; set; }
        public string? ImagemHomeUrl19 { get; set; }
        public string? ImagemHomeUrl20 { get; set; }
        public string? ServerAddress { get; set; }
        public string? NomeCondominio { get; set; }
        public string? CnpjCondominio { get; set; }
        public string? EnderecoCondominio { get; set; }

        public string? NomeAdministradoraCondominio { get; set; }
        public string? CnpjAdministradoraCondominio { get; set; }
        public string? EnderecoAdministradoraCondominio { get; set; }
        public int? EmpreendimentoId { get; set; }
        public int? PontosRci { get; set; }
        
        #region Configurações de Reserva - Campos Obrigatórios para Hóspedes Convidados
        public EnumSimNao? ExigeEnderecoHospedeConvidado { get; set; }
        public EnumSimNao? ExigeTelefoneHospedeConvidado { get; set; }
        public EnumSimNao? ExigeDocumentoHospedeConvidado { get; set; }
        #endregion

        #region Configurações de Reserva RCI
        public EnumSimNao? PermiteReservaRciApenasClientesComContratoRci { get; set; }
        #endregion

        #region Autenticação em duas etapas (2FA)
        public EnumSimNao? Habilitar2FAPorEmail { get; set; }
        public EnumSimNao? Habilitar2FAPorSms { get; set; }
        public EnumSimNao? Habilitar2FAParaCliente { get; set; }
        public EnumSimNao? Habilitar2FAParaAdministrador { get; set; }
        #endregion

    }
}
