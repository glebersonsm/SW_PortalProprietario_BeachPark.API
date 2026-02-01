using Microsoft.AspNetCore.Http;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Models.SystemModels
{
    public class ParametroSistemaInputUpdateModel
    {
        public string? SiteParaReserva { get; set; }
        public EnumSimNao? AgruparCertidaoPorCliente { get; set; }
        public EnumSimNao? EmitirCertidaoPorUnidCliente { get; set; }
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
        public ICollection<IFormFile>? Imagem1 { get; set; }
        public ICollection<IFormFile>? Imagem2 { get; set; }
        public ICollection<IFormFile>? Imagem3 { get; set; }
        public ICollection<IFormFile>? Imagem4 { get; set; }
        public ICollection<IFormFile>? Imagem5 { get; set; }
        public ICollection<IFormFile>? Imagem6 { get; set; }
        public ICollection<IFormFile>? Imagem7 { get; set; }
        public ICollection<IFormFile>? Imagem8 { get; set; }
        public ICollection<IFormFile>? Imagem9 { get; set; }
        public ICollection<IFormFile>? Imagem10 { get; set; }
        public ICollection<IFormFile>? Imagem11 { get; set; }
        public ICollection<IFormFile>? Imagem12 { get; set; }
        public ICollection<IFormFile>? Imagem13 { get; set; }
        public ICollection<IFormFile>? Imagem14 { get; set; }
        public ICollection<IFormFile>? Imagem15 { get; set; }
        public ICollection<IFormFile>? Imagem16 { get; set; }
        public ICollection<IFormFile>? Imagem17 { get; set; }
        public ICollection<IFormFile>? Imagem18 { get; set; }
        public ICollection<IFormFile>? Imagem19 { get; set; }
        public ICollection<IFormFile>? Imagem20 { get; set; }

        public string? NomeCondominio { get; set; }
        public string? CnpjCondominio { get; set; }
        public string? EnderecoCondominio { get; set; }
        public string? ExibirFinanceirosDasEmpresaIds { get; set; }
        public string? NomeAdministradoraCondominio { get; set; }
        public string? CnpjAdministradoraCondominio { get; set; }
        public string? EnderecoAdministradoraCondominio { get; set; }
        public int? PontosRci { get; set; }
        
        #region Configurações de Reserva - Campos Obrigatórios para Hóspedes Convidados
        public EnumSimNao? ExigeEnderecoHospedeConvidado { get; set; }
        public EnumSimNao? ExigeTelefoneHospedeConvidado { get; set; }
        public EnumSimNao? ExigeDocumentoHospedeConvidado { get; set; }
        #endregion

        #region Configurações de Reserva RCI
        public EnumSimNao? PermiteReservaRciApenasClientesComContratoRci { get; set; }
        #endregion

    }
}
