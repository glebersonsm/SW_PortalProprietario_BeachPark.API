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

            Map(b => b.TipoContratoIds).Length(4000).Nullable();
            Map(b => b.TipoSemanaCedida).Length(4000).Not.Nullable();
            Map(b => b.TiposSemanaPermitidosUso).Length(4000).Not.Nullable();
            Map(b => b.DataInicioVigenciaCriacao).Not.Nullable();
            Map(b => b.DataFimVigenciaCriacao).Nullable();
            Map(b => b.DataInicioVigenciaUso).Not.Nullable();
            Map(b => b.DataFimVigenciaUso).Nullable();
            Map(b => b.TiposUhIds).Length(4000).Nullable();

            Schema("portalohana");
            Table("RegraIntercambio");
        }
    }
}
