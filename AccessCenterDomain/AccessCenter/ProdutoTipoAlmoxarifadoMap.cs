using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class ProdutoTipoAlmoxarifadoMap : ClassMap<ProdutoTipoAlmoxarifado>
    {
        public ProdutoTipoAlmoxarifadoMap()
        {
            Id(x => x.Id).GeneratedBy.Sequence("PRODUTOTIPOALMOXARIFADO_");
            Map(b => b.Tag);
            Map(b => b.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.UsuarioAlteracao);
            Map(b => b.DataHoraAlteracao);

            Map(b => b.Produto);
            Map(b => b.TipoAlmoxarifado);

            Table("ProdutoTipoAlmoxarifado");
        }
    }
}
