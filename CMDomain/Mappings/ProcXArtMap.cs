using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class ProcXArtMap : ClassMap<ProcXArt>
    {
        public ProcXArtMap()
        {
            CompositeId()
                .KeyProperty(p => p.IdProcXArt)
                .KeyProperty(p => p.CodProcesso);

            Map(p => p.QtdePedida);
            Map(p => p.CodMedida);
            Map(p => p.CodArtigo);
            Map(p => p.Justificativa);
            Map(p => p.DataNecessidade);
            Map(p => p.IdProdVari);
            Map(p => p.TrgDtInclusao);
            Map(p => p.TrgUserInclusao);

            Table("ProcXArt");
        }
    }
}
