using FluentNHibernate.Mapping;
using SW_PortalProprietario.Domain.Entities.Core.Geral;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.Geral
{
    public class EstadoMap : ClassMap<Estado>
    {
        public EstadoMap()
        {

            Id(x => x.Id).GeneratedBy.Native("Estado_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.DataHoraAlteracao).Nullable();
            Map(p => p.UsuarioAlteracao).Nullable();
            Map(b => b.CodigoIbge).Length(10);
            Map(b => b.Nome).Length(100);
            Map(b => b.Sigla).Length(20);
            References(p => p.Pais, "Pais");

            Schema("portalohana");
            Table("Estado");
        }
    }
}
