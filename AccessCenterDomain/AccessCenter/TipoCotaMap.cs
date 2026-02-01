using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class TipoCotaMap : ClassMap<TipoCota>
    {
        public TipoCotaMap()
        {
            Id(x => x.Id).GeneratedBy.Sequence("TIPOCOTA_");
            Map(b => b.Tag);
            Map(b => b.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.UsuarioAlteracao);
            Map(b => b.DataHoraAlteracao);

            Map(b => b.Empreendimento);
            Map(b => b.Codigo);
            Map(b => b.Nome);
            Map(b => b.NomePesquisa);
            Map(b => b.LiberadoVenda);
            Map(b => b.QuantidadeSemana);

            Table("TipoCota");
        }
    }
}
