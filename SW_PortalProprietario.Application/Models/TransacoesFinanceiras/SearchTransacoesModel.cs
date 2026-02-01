namespace SW_PortalProprietario.Application.Models.TransacoesFinanceiras
{
    public class SearchTransacoesModel
    {
        public DateTime? DataInicial { get; set; }
        public DateTime? DataFinal { get; set; }
        public int? PessoaId { get; set; }
        public string? PessoaNome { get; set; }
        public bool? Pix { get; set; }
        public bool? Cartao { get; set; }
        public bool? RetornarContasVinculadas { get; set; }
        public int? NumeroDaPagina { get; set; }
        public int? QuantidadeRegistrosRetornar { get; set; }
        public string? StatusTransacao { get; set; }
        public int? EmpresaId { get; set; }

    }
}
