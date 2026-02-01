namespace SW_PortalProprietario.Application.Models.Empreendimento
{
    public class SearchImovelModel
    {
        public string? NumeroImovel { get; set; }
        public string? CodigoBloco { get; set; }
        public int? NumeroDaPagina { get; set; }
        public int? QuantidadeRegistrosRetornar { get; set; }

    }
}
