using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class FornXDesembMap : ClassMap<FornXDesemb>
    {
        public FornXDesembMap()
        {
            Id(x => x.IdFornXDesemb).GeneratedBy.Sequence("SEQFORNXDESEMB");
            Map(x => x.CodTipRecDes);
            Map(x => x.IdPessoa);
            Map(x => x.RecPag);
            Map(x => x.IdEmpresaProp);

            Map(p => p.TrgDtInclusao);
            Map(p => p.TrgUserInclusao);

            Table("FornXDesemb");
        }
    }
}
