using FluentNHibernate.Mapping;


namespace EsolutionPortalDomain.Portal
{
    public class CotaProprietarioMap : ClassMap<CotaProprietario>
    {
        public CotaProprietarioMap()
        {
            Id(x => x.Id)
            .GeneratedBy.SequenceIdentity();

            Map(p => p.Cota);
            Map(b => b.UhCondominio);
            Map(b => b.DataHoraInclusao);
            Map(b => b.TipoContrato);
            Map(b => b.ValorTaxaCondominio);
            Map(b => b.Tag);
            Map(b => b.BloqueioAgendamentoManual);
            Map(b => b.BloqueioManualFinanceiro);

            Table("CotaProprietario");
        }
    }
}
