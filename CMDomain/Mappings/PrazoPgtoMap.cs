using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class PrazoPgtoMap : ClassMap<PrazoPgto>
    {
        public PrazoPgtoMap()
        {
            CompositeId().KeyProperty(p => p.CodProcesso)
                .KeyProperty(p => p.IdProcXArt)
                .KeyProperty(p => p.Proposta)
                .KeyProperty(p => p.IdPrazoPgto)
                .KeyProperty(p => p.IdForCli);

            Map(p => p.Percentual);
            Map(p => p.Valor);
            Map(p => p.PeriodoPrazo);
            Map(p => p.DataPgto);
            Map(p => p.Prazopgto);
            Map(p => p.FlgAdiantamento);
            Map(p => p.TrgDtInclusao);
            Map(p => p.TrgUserInclusao);

            Table("PrazoPgto");
            Schema("cm");
        }
    }
}
