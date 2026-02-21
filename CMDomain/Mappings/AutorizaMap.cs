using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class AutorizaMap : ClassMap<Autoriza>
    {
        public AutorizaMap()
        {
            CompositeId()
                .KeyProperty(p => p.IdEspAcesso)
                .KeyProperty(p => p.IdOperFunc)
                .KeyProperty(p => p.IdPessoa);

            Map(p => p.FlgHabilita);
            Map(p => p.FlgVisualiza);

            Map(b => b.TrgDtInclusao);
            Map(b => b.TrgUserInclusao);

            Schema("cm");
            Table("Autoriza");
        }
    }
}
