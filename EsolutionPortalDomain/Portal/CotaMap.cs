using FluentNHibernate.Mapping;


namespace EsolutionPortalDomain.Portal
{
    public class CotaMap : ClassMap<Cota>
    {
        public CotaMap()
        {
            Id(x => x.Id)
            .GeneratedBy.SequenceIdentity();

            Map(p => p.Nome);
            Map(b => b.GrupoCotas);
            Map(b => b.Percentagem);
            Map(b => b.UhCondominio);
            Map(b => b.TipoCota);
            Map(b => b.PrioridadeAgendamento);

            Table("Cota");
        }
    }
}
