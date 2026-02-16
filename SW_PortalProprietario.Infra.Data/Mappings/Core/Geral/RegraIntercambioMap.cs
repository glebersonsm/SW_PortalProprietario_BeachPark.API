using FluentNHibernate.Mapping;
using SW_PortalProprietario.Domain.Entities.Core.Geral;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.Geral
{
    public class RegraIntercambioMap : ClassMap<RegraIntercambio>
    {
        public RegraIntercambioMap()
        {
            Id(x => x.Id).GeneratedBy.Native("RegraIntercambio_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.DataHoraAlteracao).Nullable();
            Map(p => p.UsuarioAlteracao).Nullable();

            Map(b => b.TipoContratoId).Nullable();
            Map(b => b.TipoSemanaCedida).Length(50).Not.Nullable();
            Map(b => b.TiposSemanaPermitidosUso).Length(500).Not.Nullable();
            Map(b => b.DataInicioVigenciaCriacao).Not.Nullable();
            Map(b => b.DataFimVigenciaCriacao).Not.Nullable();
            Map(b => b.DataInicioVigenciaUso).Not.Nullable();
            Map(b => b.DataFimVigenciaUso).Not.Nullable();

            Schema("portalohana");
            Table("RegraIntercambio");
        }
    }
}
