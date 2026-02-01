using FluentNHibernate.Mapping;
using SW_PortalProprietario.Domain.Entities.Core.Geral;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.Geral
{
    public class HistoricoRetiradaPoolMap : ClassMap<HistoricoRetiradaPool>
    {
        public HistoricoRetiradaPoolMap()
        {
            Id(x => x.Id).GeneratedBy.Native("HistoriRetiadaPool_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.DataHoraAlteracao).Nullable();
            Map(p => p.UsuarioAlteracao).Nullable();
            References(p => p.Empresa, "Empresa");
            Map(b => b.AgendamentoId);
            Map(b => b.NovoAgendamentoId);

            Table("HistoricoRetiradaPool");
        }
    }
}

