using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class FracionamentoTsMap : ClassMap<FracionamentoTs>
    {
        public FracionamentoTsMap()
        {
            Id(x => x.IdFracionamentoTs).GeneratedBy.Sequence("SEQFRACIONAMENTOTS");

            Map(p => p.IdReservasFront1);

            Map(b => b.IdReservasFront2);

            Map(b => b.IdVendaXContrato);
            Map(b => b.TrgDtInclusao);
            Map(b => b.TrgUserInclusao);

            Schema("cm");
            Table("FracionamentoTs");
        }
    }
}
