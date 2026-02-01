using SW_PortalProprietario.Domain.Entities.Core.Geral;

namespace SW_PortalProprietario.Application.Models.GeralModels
{
    public class RegistroEstadoInputModel : CreateUpdateModelBase
    {
        public string? CodigoIbge { get; set; }
        public string? Nome { get; set; }
        public string? Sigla { get; set; }
        public virtual int? PaisId { get; set; }

        public static explicit operator Estado(RegistroEstadoInputModel model)
        {
            return new Estado
            {
                CodigoIbge = model.CodigoIbge,
                Nome = model.Nome,
                Pais = new Pais() { Id = model.PaisId.GetValueOrDefault() },
                Sigla = model.Sigla
            };
        }
    }
}
