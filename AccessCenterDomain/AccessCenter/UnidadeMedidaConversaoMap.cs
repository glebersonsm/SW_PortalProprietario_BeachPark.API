using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class UnidadeMedidaConversaoMap : ClassMap<UnidadeMedidaConversao>
    {
        public UnidadeMedidaConversaoMap()
        {
            Id(x => x.Id).GeneratedBy.Sequence("UNIDADEMEDIDACONVERSAO_");
            Map(b => b.Tag);
            Map(b => b.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.UsuarioAlteracao);
            Map(b => b.DataHoraAlteracao);

            Map(b => b.Conversao);
            References(b => b.UnidadeMedida, "UnidadeMedida");
            References(b => b.UnidadeMedidaBase, "UnidadeMedidaBase");

            Table("UnidadeMedidaConversao");
        }
    }
}
