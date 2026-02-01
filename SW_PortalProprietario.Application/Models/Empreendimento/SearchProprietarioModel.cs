namespace SW_PortalProprietario.Application.Models.Empreendimento
{
    public class SearchProprietarioModel
    {
        public int? PessoaProviderId { get; set; }
        public string? Nome { get; set; }
        public string? NumeroUnidade { get; set; }
        public string? FracaoCota { get; set; }
        public string? DocumentoCliente { get; set; }
        public DateTime? DataAquisicaoInicial { get; set; }
        public DateTime? DataAquisicaoFinal { get; set; }
        public string? NumeroContrato { get; set; }
        public int? NumeroDaPagina { get; set; }
        public int? QuantidadeRegistrosRetornar { get; set; }
        public int? EmpresaId { get; set; }
        public string? StatusAssinaturaContratoSCP { get; set; }

    }
}
