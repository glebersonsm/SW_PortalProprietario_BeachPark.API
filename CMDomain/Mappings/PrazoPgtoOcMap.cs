using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class PrazoPgtoOcMap : ClassMap<PrazoPgtoOc>
    {
        public PrazoPgtoOcMap()
        {
            CompositeId().KeyProperty(p => p.IdItemOc)
                .KeyProperty(p => p.ParcelaPgto);

            Map(p => p.PrazoPgto);
            Map(p => p.PercPagto);
            Map(p => p.PeriodoPrazo);
            Map(p => p.ValorPagto);
            Map(p => p.DataPagto);
            Map(p => p.FlgAdiantamento);
            Map(p => p.TrgDtInclusao);
            Map(p => p.TrgUserInclusao);

            Table("PrazoPgtoOc");
        }
    }
}
