using FluentNHibernate.Mapping;
using SW_PortalProprietario.Domain.Entities.Core.Geral;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.Geral
{
    public class HistoricoTrocaDeSemanaMap : ClassMap<HistoricoTrocaDeSemana>
    {
        public HistoricoTrocaDeSemanaMap()
        {
            Id(x => x.Id).GeneratedBy.Native("HistoricoTrocaSemana_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.DataHoraAlteracao).Nullable();
            Map(p => p.UsuarioAlteracao).Nullable();
            References(p => p.Empresa, "Empresa");
            Map(b => b.AgendamentoAnteriorId);
            Map(b => b.NovoAgendamentoId);
            Map(b => b.Descricao).Length(4000);

            Table("HistoricoTrocaDeSemana");
        }
    }
}

