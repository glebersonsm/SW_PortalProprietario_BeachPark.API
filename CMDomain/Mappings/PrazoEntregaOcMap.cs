using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class PrazoEnregaOcMap : ClassMap<PrazoEntregaOc>
    {
        public PrazoEnregaOcMap()
        {
            CompositeId().KeyProperty(p => p.IdItemOc)
                .KeyProperty(p => p.ParcelaEntrega);

            Map(p => p.PrazoEntrega);
            Map(p => p.QtdeEntrega);
            Map(p => p.PeriodoPrazo);
            Map(p => p.DataEntrega);
            Map(p => p.TrgDtInclusao);
            Map(p => p.TrgUserInclusao);

            Table("PrazoEntregaOc");
            Schema("cm");
        }
    }
}
