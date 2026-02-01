using FluentNHibernate.Mapping;


namespace EsolutionPortalDomain.Portal
{
    public class PessoaEnderecoMap : ClassMap<PessoaEndereco>
    {
        public PessoaEnderecoMap()
        {
            Id(x => x.Id)
            .GeneratedBy.SequenceIdentity();

            Map(p => p.Pessoa);
            Map(b => b.TipoEndereco);
            Map(b => b.Logradouro);
            Map(b => b.Bairro);
            Map(b => b.Numero);
            Map(b => b.Cep);
            Map(b => b.Complemento);
            Map(b => b.Cidade);
            Map(b => b.Preferencial);
            Map(b => b.Cobranca);

            Table("PessoaEndereco");
        }
    }
}
