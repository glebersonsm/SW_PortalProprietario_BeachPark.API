using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class EspAcessMap : ClassMap<EspAcess>
    {
        public EspAcessMap()
        {
            Id(x => x.IdEspAcesso).GeneratedBy.Sequence("SEQESPACESS");

            Map(p => p.IdImagem).Nullable();
            Map(p => p.CorFundoMain);
            Map(p => p.FlgClassicViewer);
            Map(p => p.TrgUserInclusao);
            Map(p => p.TrgDtInclusao);

            Table("EspAcess");
        }
    }
}
