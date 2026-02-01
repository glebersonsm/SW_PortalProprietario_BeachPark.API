using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class ProdutoMap : ClassMap<Produto>
    {
        public ProdutoMap()
        {
            Id(x => x.Id).GeneratedBy.Sequence("PRODUTO_");
            Map(b => b.Tag);
            Map(b => b.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.UsuarioAlteracao);
            Map(b => b.DataHoraAlteracao);

            Map(b => b.GrupoEmpresa);
            Map(b => b.Codigo);
            Map(b => b.Nome);
            Map(b => b.DadosComplementares);
            References(b => b.GrupoProduto, "GrupoProduto");
            Map(b => b.GrupoTributacao);
            References(b => b.TipoProduto, "TipoProduto");
            Map(b => b.PermitirEstoquePendente);
            Map(b => b.Status);
            Map(b => b.FichaTecnica);
            Map(b => b.ProdutoVenda);
            References(b => b.Ncm, "Ncm");
            Map(b => b.AtivoFixo);
            Map(b => b.AtivoFixoTipo);
            Map(b => b.Vacina);
            Map(b => b.VacinaAgrodefesa);
            Map(b => b.TipoItemSped);
            Map(b => b.GeneroItemSped);
            Map(b => b.ProibidoVendaMenorIdade);
            Map(b => b.CombustivelLubrificante);
            Map(b => b.PercentualDesconto);
            Map(b => b.PercentualDespesaOperacional);
            Map(b => b.AliquotaIcms);
            Map(b => b.FatorCalculo);
            Map(b => b.Peso);
            Map(b => b.VariacaoParaMais);
            Map(b => b.VariacaoParaMenos);
            References(b => b.UnidadeMedidaVenda, "UnidadeMedidaVenda");
            Map(b => b.PermiteReterInss);
            Map(b => b.TipoComposicao);
            References(b => b.UnidadeMedidaBase, "UnidadeMedidaBase");
            References(b => b.ProdutoUnidadeMedidaRelatorio, "ProdutoUnidadeMedidaRelatorio");
            Map(b => b.PermiteSuspensaoPisCofins);
            References(b => b.UnidadeMedidaEntradaNota, "UnidadeMedidaEntradaNota");
            Map(b => b.QuantidadeMaximaVenda);
            Map(b => b.NomePesquisa);
            Map(b => b.Energia);
            Map(b => b.NaturezaReceitaPis);
            Map(b => b.NaturezaReceitaCofins);
            Map(b => b.ClassificacaoServicoPrestado);
            Map(b => b.DescricaoProdutoANP);
            Map(b => b.RepasseInterestadual);
            Map(b => b.PermiteDesconto);
            Map(b => b.DestinoContabil);
            Map(b => b.CategoriaProduto);
            Map(b => b.ControlaEstoque);
            Map(b => b.BloqueadoProcessoCompras);
            Map(b => b.ConsideraCustoCargaDescarga);
            Map(b => b.CEST);
            Map(b => b.DesmembraComposicaoPortal);
            Map(b => b.BaixaIndividualProdutoCom);
            Map(b => b.TempoPreparo);

            Table("Produto");
        }
    }
}
