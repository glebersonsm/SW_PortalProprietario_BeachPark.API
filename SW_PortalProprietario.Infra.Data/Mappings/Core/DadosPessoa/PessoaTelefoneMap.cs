using FluentNHibernate.Mapping;
using NHibernate.Type;
using SW_PortalProprietario.Domain.Entities.Core.DadosPessoa;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.DadosPessoa
{
    public class PessoaTelefoneMap : ClassMap<PessoaTelefone>
    {
        public PessoaTelefoneMap()
        {
            Id(x => x.Id).GeneratedBy.Native("PessoaTelefone_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.DataHoraAlteracao).Nullable();
            Map(p => p.UsuarioAlteracao).Nullable();
            References(b => b.Pessoa, "Pessoa");
            References(b => b.TipoTelefone, "TipoTelefone");
            Map(b => b.Numero);
            Map(b => b.NumeroFormatado);
            Map(b => b.Preferencial).CustomType<EnumType<EnumSimNao>>();
            Schema("portalohana");
            Table("PessoaTelefone");
        }
    }
}
