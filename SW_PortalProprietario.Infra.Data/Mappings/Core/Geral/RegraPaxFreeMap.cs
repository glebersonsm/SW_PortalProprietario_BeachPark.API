using FluentNHibernate.Mapping;
using SW_PortalProprietario.Domain.Entities.Core.Geral;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.Geral
{
    public class RegraPaxFreeMap : ClassMap<RegraPaxFree>
    {
        public RegraPaxFreeMap()
        {
            Id(x => x.Id).GeneratedBy.Native("RegraPaxFree_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.DataHoraAlteracao).Nullable();
            Map(p => p.UsuarioAlteracao).Nullable();
            Map(p => p.UsuarioRemocao).Nullable();
            Map(p => p.DataHoraRemocao).Nullable();

            Map(b => b.Nome).Length(200);
            Map(b => b.DataInicioVigencia).Nullable();
            Map(b => b.DataFimVigencia).Nullable();

            Schema("portalohana");
            Table("RegraPaxFree");
        }
    }
}

