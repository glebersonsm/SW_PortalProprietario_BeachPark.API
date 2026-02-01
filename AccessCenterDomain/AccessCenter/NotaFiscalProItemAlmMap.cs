using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class NotaFiscalProItemAlmMap : ClassMap<NotaFiscalProItemAlm>
    {
        public NotaFiscalProItemAlmMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("NOTAFISCALPROITEMALM_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(b => b.NotaFiscal);
            Map(b => b.ProdutoItemAlmoxarifado);
            Map(b => b.ProdutoUnidadeMedida);
            Map(b => b.CFOP);
            Map(b => b.SituacaoTributaria);
            Map(b => b.TributacaoSitTri);
            Map(b => b.CentroCusto);
            Map(b => b.DestinoContabil);
            Map(b => b.AtividadeProjeto);
            Map(b => b.ProdutoItemLote);
            Map(b => b.Sequencia);
            Map(b => b.HistoricoOrdemCompra);
            Map(b => b.ParametroPisCofins);
            Map(b => b.Fomentar);
            Map(b => b.FilialDestino);
            Map(b => b.TributacaoSitTriBasSug);
            Map(b => b.Observacao);
            Map(b => b.ObservacaoSugestaoTributacao);
            Map(b => b.Quantidade);
            Map(b => b.QuantidadePeca);
            Map(b => b.ValorTotalProduto);
            Map(b => b.ValorTotalCusto);
            Map(b => b.ValorFrete);
            Map(b => b.ValorSeguro);
            Map(b => b.ValorDespesasAcessorias);
            Map(b => b.ValorDespesasAcessoriasOrig);
            Map(b => b.ValorAbatimento);
            Map(b => b.ValorDesconto);
            Map(b => b.ValorDespesasForaNota);
            Map(b => b.ValorDiferencaICMS);
            Map(b => b.ValorDiferencaICMSFrete);
            Map(b => b.ValorBaseCalculoICMS);
            Map(b => b.AliquotaICMS);
            Map(b => b.ValorICMS);
            Map(b => b.ValorOutrosICMS);
            Map(b => b.ValorIsentoICMS);
            Map(b => b.ValorArredondamentoICMS);
            Map(b => b.ValorICMSDesonerado);
            Map(b => b.MotivoDesoneracaoICMS);
            Map(b => b.ValorBaseCalculoICMSOrigina);
            Map(b => b.AliquotaICMSOriginal);
            Map(b => b.CfopOriginal);
            Map(b => b.SituacaoTributariaOriginal);
            Map(b => b.LancaFinanceiro);
            Map(b => b.SugeriuTributacao);
            Map(b => b.SugerirAutomaticamente);
            Map(b => b.ValorICMSSubstituicao);
            Map(b => b.ValorIsentoIPI);
            Map(b => b.ValorOutrosIPI);
            Map(b => b.ValorBaseCalculoISS);
            Map(b => b.AliquotaISS);
            Map(b => b.ValorISS);
            Map(b => b.ValorOutrosISS);
            Map(b => b.ValorIsentoISS);
            Map(b => b.ValorINSS);
            Map(b => b.ValorSenar);
            Map(b => b.ValorINSSTomador);
            Map(b => b.ValorIRRF);
            Map(b => b.ValorPIS);
            Map(b => b.ValorCOFINS);
            Map(b => b.ValorContribuicaoSocial);
            Map(b => b.ValorDare);
            Map(b => b.ReducaoBaseCalculoICMS);
            Map(b => b.PerBaseCalculoIcms);
            Map(b => b.AliquotaBase);
            Map(b => b.PrecoVendaMargem);
            Map(b => b.PrecoVendaPIS);
            Map(b => b.PrecoVendaCOFINS);
            Map(b => b.PrecoVendaDesconto);
            Map(b => b.PrecoVendaDespesa);
            Map(b => b.PrecoVendaFator);
            Map(b => b.PrecoVendaAliquotaICMS);
            Map(b => b.PrecoVendaBaseCalculo);
            Map(b => b.PrecoVenda);
            Map(b => b.ValorConhecimentoFreteCusto);
            Map(b => b.ValorConhecimentoFreteSemICMS);

            Map(b => b.DataHoraImportacaoAtivoFixo);
            Map(b => b.CustoMedioTipoEstoquePrincipal);
            Map(b => b.GeneroItemSpedPISCOFINS);
            Map(b => b.SituacaoTributariaPIS);
            Map(b => b.SituacaoTributariaCOFINS);
            Map(b => b.SuspensaoPISCOFINS);
            Map(b => b.NumeroSeqNFOrigem);
            Map(b => b.notafisproitealmorigem);
            Map(b => b.notafisproitealmini);
            Map(b => b.NotaFiscalInicial);
            Map(b => b.SubTotal);

            Table("NotaFiscalProItemAlm");
        }
    }
}
