using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class GrupoCotaMap : ClassMap<GrupoCota>
    {
        public GrupoCotaMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("GRUPOCOTA_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);


            Map(b => b.Empreendimento);
            Map(b => b.Nome);
            Map(b => b.NomePesquisa);
            Map(p => p.Codigo);
            Map(p => p.LiberadoVenda);

            Table("GrupoCota");
        }
    }
}
