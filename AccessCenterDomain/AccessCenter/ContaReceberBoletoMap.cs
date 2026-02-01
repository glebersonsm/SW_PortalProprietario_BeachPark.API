using FluentNHibernate.Mapping;

namespace AccessCenterDomain.AccessCenter
{
    public class ContaReceberBoletoMap : ClassMap<ContaReceberBoleto>
    {
        public ContaReceberBoletoMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("CONTARECEBERBOLETO_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(p => p.Filial);
            Map(b => b.Empresa);
            Map(b => b.GrupoEmpresa);
            Map(b => b.Emissao);
            Map(b => b.Vencimento);
            Map(b => b.VencimentoOriginal);
            Map(b => b.LimitePagamentoTransmitido);
            Map(b => b.LimitePagamento);
            Map(b => b.ComLimitePagamentoTra);
            Map(b => b.ComLimitePagamento);
            Map(b => b.CodigoBarras);
            Map(b => b.LinhaDigitavel);
            Map(b => b.NossoNumero);
            Map(b => b.Sequencia);
            Map(b => b.ValorBoleto);
            Map(b => b.ValorBoletoOriginal);
            Map(b => b.PercentualJuroDiario);
            Map(b => b.ValorJuroDiario);
            Map(b => b.PercentualJuroMensal);
            Map(b => b.ValorJuroMensal);
            Map(b => b.MultaBoleto);
            Map(b => b.PercentualMulta);
            Map(b => b.ValorBaixa);
            Map(b => b.BoletoImpresso);
            Map(b => b.QuantidadeImpressoes);
            Map(b => b.Status);
            Map(b => b.Banco);
            Map(b => b.ContaFinVariConCob);
            Map(b => b.Parcela);
            Map(b => b.ManterNossoNumero);
            Map(b => b.Descricao);
            Map(b => b.QuantidadeParcelasVinculadas);
            Map(b => b.DataHoraBaixa);
            Map(b => b.UsuarioBaixa);
            Map(b => b.DataHoraCancelamento);
            Map(b => b.UsuarioCancelamento);

            Table("ContaReceberBoleto");
        }
    }
}
