using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class NaturezaOperacaoMap : ClassMap<NaturezaOperacao>
    {
        public NaturezaOperacaoMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("NATUREZAOPERACAO_SEQUENCE");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(b => b.GrupoEmpresa);
            Map(b => b.Empresa);
            Map(b => b.Codigo);
            Map(b => b.Nome);
            Map(b => b.NomePesquisa);
            Map(b => b.NotaFiscalSequencia);
            Map(b => b.Status);
            Map(b => b.LancaFinanceiro);
            Map(b => b.ConsideraIcmsDesonerado);
            Map(b => b.InformaNotaFiscalOrigem);
            Map(b => b.LivroICMS);
            Map(b => b.LivroISS);
            Map(b => b.OptantePisCofins);
            Map(b => b.CalculaPisCofinsAut);
            Map(b => b.BloqueiaSemParametroPisCofins);
            Map(b => b.Tipo);
            Map(b => b.TipoNotaFiscalOrigem);
            Map(b => b.ExigeCentroCusto);
            Map(b => b.ExigeOrdemCompra);
            Map(b => b.Venda);
            Map(b => b.Bonificacao);
            Map(b => b.SugerePrecoVenda);
            Map(b => b.PermiteInformarImpostosMan);
            Map(b => b.Requisicao);
            Map(b => b.Transferencia);
            Map(b => b.PermiteDevolucao);
            Map(b => b.EfetuaPedido);
            Map(b => b.ExigeClienteEfetivo);
            Map(b => b.PermiteLancarDespesa);
            Map(b => b.PermiteRecebimentoMercadoria);
            Map(b => b.EntradaAtivoFixo);
            Map(b => b.Devolucao);
            Map(b => b.NaturezaOperacaoEntradaTrans);
            Map(b => b.NaturezaOperacaoReferenciada);
            Map(b => b.TipoContaPagarReceber);
            Map(b => b.Contabilizar);
            Map(b => b.ExigeDestinacaoContabil);
            Map(b => b.OperacaoFinanceira);
            Map(b => b.TipoCadastroContabil);
            Map(b => b.SugereINSS);
            Map(b => b.Frete);
            Map(b => b.RelatorioGerencial);
            Map(b => b.Compra);
            Map(b => b.Retorno);
            Map(b => b.NotaFiscalOrigemLancamentoUti);
            Map(b => b.ControlaLote);
            Map(b => b.PermiteFiliaisDiferentes);
            Map(b => b.PermiteFiliaisIguais);
            Map(b => b.BaseSugestaoTributacao);
            Map(b => b.PermitirSomenteUmTipoProduto);
            Map(b => b.PermitirCriarAutorizacao);
            Map(b => b.IndiceFinanceiro);
            Map(b => b.CompraEnergiaEletricaConLiv);
            Map(b => b.PermitirInformarFrete);
            Map(b => b.PermitirInformarValorFrete);
            Map(b => b.ControlaPeca);
            Map(b => b.Desfazimento);

            Table("NaturezaOperacao");
        }
    }
}
