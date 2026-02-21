using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class ReservasTsMap : ClassMap<ReservasTs>
    {
        public ReservasTsMap()
        {
            Id(x => x.IdReservasFront)
            .GeneratedBy.Assigned();

            Map(p => p.IdDisponibilidade);
            Map(p => p.DataChegada);
            Map(p => p.DataPartida);
            Map(p => p.DataConfirmacao);
            Map(p => p.IdMotivoTs);
            Map(p => p.TrgUserInclusao);
            Map(p => p.TrgDtInclusao);

            Table("ReservasTs");
            Schema("cm");
        }
    }
}
