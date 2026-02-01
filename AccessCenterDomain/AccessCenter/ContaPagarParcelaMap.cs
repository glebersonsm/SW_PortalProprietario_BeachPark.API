using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class ContaPagarParcelaMap : ClassMap<ContaPagarParcela>
    {
        public ContaPagarParcelaMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("CONTAPAGARPARCELA_SEQUENCE");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(p => p.ContaPagar);
            Map(b => b.DocumentoOutros);
            Map(b => b.Parcela);
            Map(b => b.VencimentoOriginal);
            Map(b => b.Vencimento);
            Map(b => b.NomeFavorecido);
            Map(b => b.TipoContaPagarOriginal);
            Map(b => b.TipoContaPagar);
            Map(b => b.AplicacaoCaixa);
            Map(b => b.ValorOriginal);
            Map(b => b.Valor);
            Map(b => b.ValorDesconto);
            Map(b => b.ValorJuros);
            Map(b => b.ValorMulta);
            Map(b => b.ValorTaxaCobranca);
            Map(b => b.SaldoPendente);
            Map(b => b.ValorBaixado);
            Map(b => b.ValorAmortizado);
            Map(b => b.Status);
            Map(b => b.UsuarioBaixa);
            Map(b => b.DataHoraBaixa);
            Map(b => b.PercentualMulta);
            Map(b => b.PercentualJuroDiario);
            Map(b => b.PercentualDescPagAntecipado);
            Map(b => b.DiasDescontoPagAntecipado);
            Map(b => b.ContaPagarLote);
            Map(b => b.ContaPagarRemessa);
            Map(b => b.BoletoCodigoBarras);
            Map(b => b.BoletoLinhaDigitavel);
            Map(b => b.EmitirOrdemProtesto);
            Map(b => b.DiasProtesto);
            Map(b => b.BancoTransferencia);
            Map(b => b.AgenciaTransferencia);
            Map(b => b.DigitoAgenciaTransferencia);
            Map(b => b.ContaTransferencia);
            Map(b => b.DigitoContaTransferencia);
            Map(b => b.VariacaoTransferencia);
            Map(b => b.DocumentoOrdemPagamento);
            Map(b => b.UtilizaComposicaoContabilLan);
            Map(b => b.ComposicaoContabilLancamento);
            Map(b => b.UtilizaComposicaoContabilBaixa);
            Map(b => b.ComposicaoContabilBaixa);
            Map(b => b.BloqueadoParaBaixa);
            Map(b => b.ObservacaoBloqueio);
            Map(b => b.LocalPagamento);
            Map(b => b.CidadeAgencia);
            Map(b => b.InformarFavorecido);
            Map(b => b.FavorecidoPessoaTipo);
            Map(b => b.CpfFavorecido);
            Map(b => b.OrigemContaBancaria);
            Map(b => b.TipoContaTransferencia);
            Map(b => b.PrevisaoPagamento);
            Map(b => b.ModalidadePagamento);
            Map(b => b.ClienteContaBancaria);
            Map(b => b.IntegracaoBancariaConPagReg);
            Map(b => b.StatusIntegracaoBancaria);
            Map(b => b.IdentificadorRemessaPagamento);
            Map(b => b.NossoNumero);
            Map(b => b.IntegracaoBancariaRetDDAIte);
            Map(b => b.OrdemPagamentoBanco);
            Map(b => b.OrdemPagamentoAgencia);
            Map(b => b.Mabu_BloqueioAutomatico);
            Map(b => b.ValorDescontoParaBaixa);
            Map(b => b.ValorJurosParaBaixa);
            Map(b => b.ValorMultaParaBaixa);
            Map(b => b.ValorTaxaCobrancaParaBaixa);

            Table("ContaPagarParcela");
        }
    }
}
