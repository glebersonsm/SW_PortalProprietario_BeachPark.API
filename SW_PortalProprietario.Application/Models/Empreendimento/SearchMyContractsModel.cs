namespace SW_PortalProprietario.Application.Models.Empreendimento
{
    public class SearchMyContractsModel
    {
        public int? NumeroDaPagina { get; set; }
        public int? QuantidadeRegistrosRetornar { get; set; }
        public bool? AdmVisaoDeCliente { get; set; } = false;
        public int? PessoaLegadoId { get; set; }
        public int? CotaAcId { get; set; }

    }
}
