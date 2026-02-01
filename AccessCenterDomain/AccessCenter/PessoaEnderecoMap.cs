using FluentNHibernate.Mapping;

namespace AccessCenterDomain.AccessCenter
{
    public class PessoaEnderecoMap : ClassMap<PessoaEndereco>
    {
        public PessoaEnderecoMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("PESSOAENDERECO_SEQUENCE");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(p => p.TipoEndereco);
            Map(b => b.Logradouro);
            Map(b => b.Numero);
            Map(b => b.Bairro);
            Map(b => b.Cep);
            Map(b => b.Complemento);
            Map(b => b.Preferencial);
            Map(b => b.Cobranca);
            Map(b => b.Entrega);
            Map(b => b.Cidade);

            Map(b => b.Pessoa);
            Map(b => b.Estrangeiro);
            Map(b => b.EnderecoCorreto);

            Table("PessoaEndereco");
        }
    }
}
