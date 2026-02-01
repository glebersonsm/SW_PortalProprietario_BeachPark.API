using AccessCenterDomain.AccessCenter.Fractional;
using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class FrProdutoParticipanteMap : ClassMap<FrProdutoParticipante>
    {
        public FrProdutoParticipanteMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("FRPRODUTOPARTICIPANTE_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(b => b.FrProduto);
            Map(b => b.GrupoEmpresa);
            Map(b => b.Empresa);
            Map(b => b.Filial);
            Map(b => b.FilialParticipante);
            Map(b => b.Valor);
            Map(b => b.ValorEntrada);
            Map(b => b.CancelamentoDevolverJurosRec);
            Map(b => b.CancelamentoValorMulta);
            Map(b => b.CancelamentoTaxaMulta);
            Map(b => b.QuantidadeMaximaDiasCanSemMul);
            Map(b => b.LancarAlteradorValorJuro);
            Map(b => b.TipoContaReceberCreditoCan);
            Map(b => b.TipoContaReceberCreditoRev);
            Map(b => b.TipoContaReceberTaxaCan);
            Map(b => b.TipoContaReceberValorIntAnt);
            Map(b => b.TipoBaixaEncontroContasRev);
            Map(b => b.OperacaoFinanceira);
            Map(b => b.AplicacaoCaixa);
            Map(b => b.AlteradorValorJuroPar);
            Map(b => b.TipoCalculoAntecipacao);
            Map(b => b.AlteradorValorAjusteArr);
            Map(b => b.OperacaoFinanceiraMultaCan);

            Table("FrProdutoParticipante");
        }
    }
}
