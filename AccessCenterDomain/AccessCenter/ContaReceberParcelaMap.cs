using FluentNHibernate.Mapping;

namespace AccessCenterDomain.AccessCenter
{
    public class ContaReceberParcelaMap : ClassMap<ContaReceberParcela>
    {
        public ContaReceberParcelaMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("CONTARECEBERPARCELA_SEQUENCE");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(p => p.ContaReceber);
            Map(b => b.Documento);
            Map(b => b.Parcela);
            Map(b => b.ParcelaCartao);
            Map(b => b.VencimentoOriginal);
            Map(b => b.Vencimento);
            Map(b => b.TipoContaReceberOriginal);
            Map(b => b.TipoContaReceber);
            Map(b => b.TipoParcela);
            Map(b => b.AplicacaoCaixa);
            Map(b => b.QuantidadeParcelasCartao);
            Map(b => b.ValorOriginal);
            Map(b => b.Valor);
            Map(b => b.ValorDesconto);
            Map(b => b.ValorJuros);
            Map(b => b.ValorMulta);
            Map(b => b.ValorTaxaCobranca);
            Map(b => b.SaldoPendente);
            Map(b => b.SaldoPendenteFpp);
            Map(b => b.ValorBaixado);
            Map(b => b.ValorAmortizado);
            Map(b => b.Status);
            Map(b => b.Nsu);
            Map(b => b.UsuarioBaixa);
            Map(b => b.DataHoraBaixa);
            Map(b => b.DataCobranca);
            Map(b => b.Devolucao1Data);
            Map(b => b.Devolucao2Data);
            Map(b => b.IntegracaoId);
            Map(b => b.ClienteCartaoCredito);
            Map(b => b.CartaoCreditoRecorrenteStatus);
            Map(b => b.NumeroCartaoCriptografado);
            Map(b => b.DocumentoFinanceira);
            Map(b => b.DocumentoOutros);
            Map(b => b.AutorizacaoCartao);
            Map(b => b.Financeira);
            Map(b => b.UtilizaComposicaoContabilBaixa);
            Map(b => b.UtilizaComposicaoContabilLan);

            Table("ContaReceberParcela");
        }
    }
}
