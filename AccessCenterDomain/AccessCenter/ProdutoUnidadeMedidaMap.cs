using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class ProdutoUnidadeMedidaMap : ClassMap<ProdutoUnidadeMedida>
    {
        public ProdutoUnidadeMedidaMap()
        {
            Id(x => x.Id).GeneratedBy.Sequence("PRODUTOUNIDADEMEDIDA_SEQUENCE");
            Map(b => b.Tag);
            Map(b => b.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.UsuarioAlteracao);
            Map(b => b.DataHoraAlteracao);

            Map(b => b.CodigoBarras);
            References(b => b.Produto, "Produto");
            References(b => b.UnidadeMedida, "UnidadeMedida");
            Map(b => b.UnidadeVenda);
            Map(b => b.UnidadeRelatorio);
            Map(b => b.UnidadeEntradaNotaFiscal);
            Map(b => b.UnidadeMedidaPrincipal);
            Map(b => b.PesoLiquido);
            Map(b => b.PesoBruto);

            Table("ProdutoUnidadeMedida");
        }
    }
}
