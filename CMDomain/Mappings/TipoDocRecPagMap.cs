using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class TipoDocRecPagMap : ClassMap<TipoDocRecPag>
    {
        public TipoDocRecPagMap()
        {
            Id(x => x.CodTipDoc).GeneratedBy.Assigned();

            Map(p => p.RecPag);
            Map(b => b.Descricao);
            Map(b => b.FlgServico);
            Map(p => p.FlgAtivo);
            Map(p => p.FlgDocFiscal);
            Map(p => p.CodTipDocReceb);
            Map(b => b.TrgDtInclusao);
            Map(b => b.TrgUserInclusao);

            Table("TipoDocRecPag");
            Schema("cm");
        }
    }
}
