namespace SW_PortalProprietario.Application.Models.GeralModels
{
    public class RegraPaxFreeConfiguracaoInputModel : CreateUpdateModelBase
    {
        public int? QuantidadeAdultos { get; set; }
        public int? QuantidadePessoasFree { get; set; }
        public int? IdadeMaximaAnos { get; set; }
        public string? TipoOperadorIdade { get; set; } // ">=" para superior ou igual, "<=" para inferior ou igual
        public string? TipoDataReferencia { get; set; } // "RESERVA" para data da reserva (hoje), "CHECKIN" para data de check-in
    }
}

