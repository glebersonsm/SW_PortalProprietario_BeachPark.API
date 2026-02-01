namespace SW_PortalProprietario.Application.Models.GeralModels
{
    public class AlteracaoEstadoInputModel : CreateUpdateModelBase
    {
        public string? CodigoIbge { get; set; }
        public string? Nome { get; set; }
        public string? Sigla { get; set; }
        public virtual int? PaisId { get; set; }
    }
}
