using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class GrupoCotaTipoCotaMap : ClassMap<GrupoCotaTipoCota>
    {
        public GrupoCotaTipoCotaMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("GRUPOCOTATIPOCOTA_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(b => b.Codigo);
            Map(b => b.Nome);
            Map(b => b.GrupoCota);
            Map(b => b.TipoCota);
            Map(b => b.PrioridadeAgendamentoCota);
            Map(b => b.Percentual);
            Map(b => b.SwVinculosTse);

            Table("GrupoCotaTipoCota");
        }
    }
}
