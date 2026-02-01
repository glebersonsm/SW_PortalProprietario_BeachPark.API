using FluentNHibernate.Mapping;
using SW_PortalProprietario.Domain.Entities.Core.DadosPessoa;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.DadosPessoa
{
    public class TipoTelefoneMap : ClassMap<TipoTelefone>
    {
        public TipoTelefoneMap()
        {
            Id(x => x.Id).GeneratedBy.Native("TipoTelefone_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.DataHoraAlteracao).Nullable();
            Map(p => p.UsuarioAlteracao).Nullable();
            Map(b => b.Nome).Length(100);
            Map(b => b.Mascara).Nullable();

            Table("TipoTelefone");
        }
    }
}
