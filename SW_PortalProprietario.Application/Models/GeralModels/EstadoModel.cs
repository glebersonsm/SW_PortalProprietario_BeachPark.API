
namespace SW_PortalProprietario.Application.Models.GeralModels
{
    public class EstadoModel : ModelBase
    {
        public EstadoModel()
        { }

        public virtual PaisModel? Pais { get; set; }
        public virtual string? PaisNome { get; set; }
        public virtual string? CodigoIbge { get; set; }
        public virtual string? PaisCodigoIbge { get; set; }
        public virtual string? Nome { get; set; }
        public virtual string? Sigla { get; set; }

    }
}
