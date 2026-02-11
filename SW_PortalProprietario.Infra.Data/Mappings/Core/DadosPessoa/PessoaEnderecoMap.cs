using FluentNHibernate.Mapping;
using NHibernate.Type;
using SW_PortalProprietario.Domain.Entities.Core.DadosPessoa;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.DadosPessoa
{
    public class PessoaEnderecoMap : ClassMap<PessoaEndereco>
    {
        public PessoaEnderecoMap()
        {
            Id(x => x.Id).GeneratedBy.Native("PessoaEndereco_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.DataHoraAlteracao).Nullable();
            Map(p => p.UsuarioAlteracao).Nullable();
            Map(b => b.Logradouro);
            Map(b => b.Bairro);
            Map(b => b.Numero);
            Map(b => b.Complemento);
            Map(b => b.Cep);
            Map(b => b.Preferencial).CustomType<EnumType<EnumSimNao>>();
            References(b => b.Pessoa, "Pessoa");
            References(b => b.TipoEndereco, "TipoEndereco");
            References(b => b.Cidade, "Cidade");
            Schema("portalohana");
            Table("PessoaEndereco");
        }
    }
}
