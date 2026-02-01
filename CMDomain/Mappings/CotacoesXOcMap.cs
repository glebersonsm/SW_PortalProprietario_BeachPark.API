using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class CotacoesXOcMap : ClassMap<CotacoesXOc>
    {
        public CotacoesXOcMap()
        {
            CompositeId()
                .KeyProperty(p => p.IdProcXArt)
                .KeyProperty(p => p.IdForCli)
                .KeyProperty(p => p.CodProcesso)
                .KeyProperty(p => p.Proposta)
                .KeyProperty(p => p.IdItemOc)
                .KeyProperty(p => p.NumOc);

            Map(p => p.TrgUserInclusao);
            Map(p => p.TrgDtInclusao);

            Table("CotacoesXOc");
        }
    }
}
