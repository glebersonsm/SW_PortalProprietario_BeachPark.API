namespace SW_PortalProprietario.Application.Models.GeralModels
{
    public class RegistroCidadeInputModel : CreateUpdateModelBase
    {
        public string? CodigoIbge { get; set; }
        public string? Nome { get; set; }
        public int? EstadoId { get; set; }

    }
}
