using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class MovimentacaoProItemAlmMap : ClassMap<MovimentacaoProItemAlm>
    {
        public MovimentacaoProItemAlmMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("MOVIMENTACAOPROALM_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);


            Map(b => b.TipoEstoque);
            Map(b => b.ProdutoItemAlmoxarifado);
            Map(b => b.Data);
            Map(p => p.TipoMovimentacao);
            Map(p => p.Descricao);
            Map(p => p.QuantidadeMovimento);
            Map(p => p.QuantidadeMovimentoPeca);
            Map(p => p.UnidadeMedidaMovimento);
            Map(p => p.UnidadeConvertida);
            Map(p => p.ValorMovimento);
            Map(p => p.EntradaSaida);
            Map(p => p.DataDocumentoOrigem);
            Map(p => p.TipoValorMovimentacao);
            Map(p => p.NotaFiscalProItemAlm);
            Map(p => p.NotaFiscalProItemAlmExc);
            Map(p => p.MovProItemAlmEstornada);
            Map(p => p.MovProItemAlmEstorno);
            Map(p => p.RequisicaoAlmAteProItem);
            Map(p => p.AjusteEstoqueProItemAlm);
            Map(p => p.QuantidadeSaldo);
            Map(p => p.ValorSaldo);
            Map(p => p.PrecoCusto);
            Map(p => p.Ordem);

            Table("MovimentacaoProItemAlm");
        }
    }
}
