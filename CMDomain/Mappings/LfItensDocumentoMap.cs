using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class LfItensDocumentoMap : ClassMap<LfItensDocumento>
    {
        public LfItensDocumentoMap()
        {
            Id(x => x.IdLfItensDocumento);

            Map(p => p.IdDocumento);
            Map(p => p.CodStCofins);
            Map(p => p.CodStPis);
            Map(p => p.CodStb);
            Map(p => p.CodUnidade);
            Map(p => p.CodItem);
            Map(p => p.Cfop);
            Map(p => p.Quantidade);
            Map(p => p.VlrUnitario);
            Map(p => p.VlrContabil);
            Map(p => p.VlrOutros);
            Map(p => p.TrgDtInclusao);
            Map(p => p.TrgUserInclusao);

            Table("LfItensDocumento");
        }
    }
}
