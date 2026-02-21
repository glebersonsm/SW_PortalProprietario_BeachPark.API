using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class PessoaFisicaMap : ClassMap<PessoaFisica>
    {
        public PessoaFisicaMap()
        {
            Id(x => x.IdPessoa).GeneratedBy.Assigned();

            Map(p => p.Sexo);
            Map(p => p.DataNasc);
            Map(p => p.TrgDtInclusao);
            Map(p => p.TrgUserInclusao);

            Table("PessoaFisica");
            Schema("cm");
        }
    }
}
