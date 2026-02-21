using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class EndPessMap : ClassMap<EndPess>
    {
        public EndPessMap()
        {
            Id(x => x.IdEndereco).GeneratedBy.Sequence("SEQENDPESS");

            Map(p => p.IdPessoa);

            Map(b => b.IdCidades);

            Map(b => b.Logradouro);
            Map(b => b.Numero);
            Map(b => b.Complemento);
            Map(b => b.Bairro);

            Map(b => b.Cep);
            Map(b => b.TipoEndereco);
            Map(b => b.Nome);
            Map(b => b.FlgTipoEnd);

            Map(b => b.TrgDtInclusao);
            Map(b => b.TrgUserInclusao);
            Map(b => b.TrgDtAlteracao);
            Map(b => b.TrgUserAlteracao);

            Schema("cm");
            Table("EndPess");
        }
    }
}
