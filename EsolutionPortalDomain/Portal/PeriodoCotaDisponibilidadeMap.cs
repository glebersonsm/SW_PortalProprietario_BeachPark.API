using FluentNHibernate.Mapping;


namespace EsolutionPortalDomain.Portal
{
    public class PeriodoCotaDisponibilidadeMap : ClassMap<PeriodoCotaDisponibilidade>
    {
        public PeriodoCotaDisponibilidadeMap()
        {
            Id(x => x.Id)
            .GeneratedBy.SequenceIdentity();

            Map(p => p.DataInicial);
            Map(b => b.DataFinal);
            Map(b => b.UhCondominio);
            Map(b => b.DataHoraInclusao);
            Map(b => b.DataHoraExclusao);
            Map(b => b.TipoDisponibilizacao);
            Map(b => b.Cota);
            Map(b => b.UsuarioInclusao);
            Map(b => b.UsuarioExclusao);
            Map(b => b.Observacao);

            Table("PeriodoCotaDisponibilidade");
        }
    }
}
