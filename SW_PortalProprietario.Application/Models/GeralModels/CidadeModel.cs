namespace SW_PortalProprietario.Application.Models.GeralModels
{
    public class CidadeModel : ModelBase
    {

        public int? PaisId { get; set; }
        public string? PaisNome { get; set; }
        public string? PaisCodigoIbge { get; set; }
        public string? EstadoSigla { get; set; }
        public string? EstadoCodigoIbge { get; set; }
        public int? EstadoId { get; set; }
        public string? EstadoNome { get; set; }
        public string? Nome { get; set; }
        public string? CodigoIbge { get; set; }
        public string? NomeFormatado { get; set; }

    }
}

