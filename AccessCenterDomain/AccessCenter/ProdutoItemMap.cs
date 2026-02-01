using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class ProdutoItemMap : ClassMap<ProdutoItem>
    {
        public ProdutoItemMap()
        {
            Id(x => x.Id).GeneratedBy.Sequence("PRODUTOITEM_");
            Map(b => b.Tag);
            Map(b => b.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.UsuarioAlteracao);
            Map(b => b.DataHoraAlteracao);

            Map(b => b.Codigo);
            Map(b => b.Nome);
            Map(b => b.CodigoBarras);
            References(b => b.Produto, "Produto");
            Map(b => b.NomePesquisa);
            Map(b => b.NomeProduto);
            Map(b => b.NomeProdutoPesquisa);
            Map(b => b.PrecoVenda);
            Map(b => b.PrecoPauta);
            Map(b => b.CodigoBalanca);
            Map(b => b.Sequencia);
            Map(b => b.TipoSubsidio);
            Map(b => b.ValorSubsidio);

            Table("ProdutoItem");
        }
    }
}
