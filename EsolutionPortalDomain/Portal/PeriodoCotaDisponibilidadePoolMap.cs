using FluentNHibernate.Mapping;


namespace EsolutionPortalDomain.Portal
{
    public class PeriodoCotaDisponibilidadePoolMap : ClassMap<PeriodoCotaDisponibilidadePool>
    {
        public PeriodoCotaDisponibilidadePoolMap()
        {
            Id(x => x.Id)
            .GeneratedBy.SequenceIdentity();

            Map(b => b.DataHoraInclusao);
            Map(b => b.DataHoraExclusao);
            Map(b => b.UsuarioInclusao);
            Map(b => b.UsuarioExclusao);
            Map(b => b.PeriodoCotaDisponibilidade);

            Table("PeriodoCotaDisponibilidadePool");
        }
    }
}
