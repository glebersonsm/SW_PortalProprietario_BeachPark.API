using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class ReservasRciMap : ClassMap<ReservasRci>
    {
        public ReservasRciMap()
        {
            Id(x => x.IdReservasRci)
            .GeneratedBy.Sequence("SEQRESERVASRCI");

            Map(p => p.IdReservasFront);
            Map(p => p.IdReservaMigrada);
            Map(p => p.FlgBulk);
            Map(p => p.TrgUserInclusao);
            Map(p => p.TrgDtInclusao);

            Table("ReservasRci");
            Schema("cm");
        }
    }
}
