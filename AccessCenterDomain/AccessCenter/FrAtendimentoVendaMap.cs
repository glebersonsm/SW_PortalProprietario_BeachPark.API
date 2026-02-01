using AccessCenterDomain.AccessCenter.Fractional;
using FluentNHibernate.Mapping;

namespace AccessCenterDomain.AccessCenter
{
    public class FrAtendimentoVendaMap : ClassMap<FrAtendimentoVenda>
    {
        public FrAtendimentoVendaMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("FRATENDIMENTOVENDA_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(p => p.Filial);
            Map(b => b.IdIntercambiadora);
            Map(b => b.IntegracaoId);
            Map(b => b.Codigo);
            Map(b => b.FrSala);
            Map(b => b.FrAtendimento);
            Map(b => b.FrPessoa1);
            Map(b => b.FrPessoa2);
            Map(b => b.FrProduto);
            Map(b => b.Cota);
            Map(b => b.DataVenda);
            Map(b => b.Valor);
            Map(b => b.ValorBaseComissao);
            Map(b => b.ValorFinanciado);
            Map(b => b.ValorAmortizado);
            Map(b => b.ValorPonto);
            Map(b => b.QuantidaPontos);
            Map(b => b.TempoUtilizacao);
            Map(b => b.Status);
            Map(b => b.DataCancelamento);
            Map(b => b.BeBack);
            Map(b => b.ProdutoEntregue);
            Map(b => b.DataEntregaProduto);
            Map(b => b.GrupoEmpresa);
            Map(b => b.Empresa);
            Map(b => b.PessoaIndicacao);
            Map(b => b.SequenciaVendaCota);
            Map(b => b.Contigencia);
            Map(b => b.Gaveta);
            Map(b => b.CotaOriginal);
            Map(b => b.FrMotivoCancelamento);
            Map(b => b.VirouComissaoV2);
            Map(b => b.FrAtendimentoVendaOrigemRev);
            Map(b => b.ContaReceberCreditoCan);
            Map(b => b.ContaReceberMultaCancelamento);
            Map(b => b.DataReversao);
            Map(b => b.UsuarioReversao);
            Map(b => b.ValorConvertidoFinanciado);
            Map(b => b.ValorConvertidoAmortizado);
            Map(b => b.SaldoReceber);
            Map(b => b.DataContigencia);
            Map(b => b.TipoMoeda);
            Map(b => b.SemFinanceiro);
            Map(b => b.IdPromotorTlmkt);
            Map(b => b.IdPromotor);
            Map(b => b.IdLiner);
            Map(b => b.IdCloser);
            Map(b => b.IdPep);
            Map(b => b.IdFtbSugerido);
            Map(b => b.IdLinerSugerido);
            Map(b => b.IdCloserSugerido);
            Map(b => b.IdPepSugerido);
            Map(b => b.FlgFtb);

            Table("FrAtendimentoVenda");
        }
    }
}
