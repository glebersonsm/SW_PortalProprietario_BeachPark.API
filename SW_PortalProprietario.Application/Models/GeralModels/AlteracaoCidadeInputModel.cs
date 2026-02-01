namespace SW_PortalProprietario.Application.Models.GeralModels
{
    public class AlteracaoCidadeInputModel : CreateUpdateModelBase
    {
        public string? CodigoIbge { get; set; }
        public string? Nome { get; set; }
        public virtual int? EstadoId { get; set; }
    }
}
