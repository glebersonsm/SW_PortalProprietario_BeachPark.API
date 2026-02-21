using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class PessoaMap : ClassMap<Pessoa>
    {
        public PessoaMap()
        {
            Id(x => x.IdPessoa).GeneratedBy.Sequence("SEQPESSOA");

            Map(p => p.IdDocumento).Nullable();
            Map(p => p.Nome);
            Map(p => p.Tipo);
            Map(p => p.RazaoSocial);
            Map(p => p.FlgUsuario);
            Map(p => p.FlgCliente);
            Map(p => p.FlgOutro);
            Map(p => p.FlgTerceiro);
            Map(p => p.FlgFornServ);
            Map(p => p.FlgFuncionario);
            Map(p => p.FlgEstrangeiro);
            Map(p => p.FlgProdutor);
            Map(p => p.FlgAgencia);
            Map(p => p.FlgBanco);
            Map(p => p.NumDocumento);
            Map(p => p.Email);
            Map(p => p.IdEndCorresp).Nullable();
            Map(p => p.IdEndComercial).Nullable();
            Map(p => p.IdEndEntrega).Nullable();
            Map(p => p.IdEndResidencial).Nullable();
            Map(p => p.IdEndCobranca).Nullable();
            Map(p => p.TrgDtInclusao);
            Map(p => p.TrgUserInclusao);

            Table("Pessoa");
            Schema("cm");
        }
    }
}
