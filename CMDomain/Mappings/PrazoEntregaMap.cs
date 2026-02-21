using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class PrazoEnregaMap : ClassMap<PrazoEntrega>
    {
        public PrazoEnregaMap()
        {
            CompositeId().KeyProperty(p => p.CodProcesso)
                .KeyProperty(p => p.IdProcXArt)
                .KeyProperty(p => p.Proposta)
                .KeyProperty(p => p.IdPrazoEnt)
                .KeyProperty(p => p.IdForCli);

            Map(p => p.QtdeEnt);
            Map(p => p.CodMedida);
            Map(p => p.PeriodoPrazo);
            Map(p => p.DataEnt);
            Map(p => p.PrazoEnt);
            Map(p => p.TrgDtInclusao);
            Map(p => p.TrgUserInclusao);

            Table("PrazoEntrega");
            Schema("cm");
        }
    }
}
