using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class DocPessoaMap : ClassMap<DocPessoa>
    {
        public DocPessoaMap()
        {
            CompositeId()
                .KeyProperty(p => p.IdDocumento)
                .KeyProperty(p => p.IdPessoa);

            Map(p => p.NumDocumento);
            Map(p => p.Orgao);
            Map(p => p.DataEmissao);
            Map(p => p.DataValidade);
            Map(p => p.IdEstado);
            Map(p => p.TrgUserInclusao);
            Map(p => p.TrgDtInclusao);
            Map(p => p.TrgUserAlteracao);
            Map(p => p.TrgDtAlteracao);


            Table("DocPessoa");
        }
    }
}
