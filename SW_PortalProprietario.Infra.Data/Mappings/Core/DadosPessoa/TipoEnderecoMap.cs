using FluentNHibernate.Mapping;
using SW_PortalProprietario.Domain.Entities.Core.DadosPessoa;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.DadosPessoa
{
    public class TipoEnderecoMap : ClassMap<TipoEndereco>
    {
        public TipoEnderecoMap()
        {
            Id(x => x.Id).GeneratedBy.Native("TipoEndereco_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.DataHoraAlteracao).Nullable();
            Map(p => p.UsuarioAlteracao).Nullable();
            Map(b => b.Nome).Length(200);
            Schema("portalohana");
            Table("TipoEndereco");
        }
    }
}
