using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class TipoContaReceberMap : ClassMap<TipoContaReceber>
    {
        public TipoContaReceberMap()
        {
            Id(x => x.Id).GeneratedBy.Sequence("TIPOCONTARECEBER_SEQUENCE");
            Map(b => b.Tag);
            Map(b => b.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.UsuarioAlteracao);
            Map(b => b.DataHoraAlteracao);

            Map(b => b.Empresa);
            Map(b => b.GrupoEmpresa);
            Map(b => b.Codigo);
            Map(b => b.Nome);
            Map(b => b.NomePesquisa);
            Map(b => b.Status);
            Map(b => b.Adiantamento);
            Map(b => b.Finalizadora);
            Map(b => b.BaixaAutomatica);
            Map(b => b.lancaMovFinanceiraRecbto);
            Map(b => b.AFaturar);
            Map(b => b.PermitirLactoDebito);
            Map(b => b.PermitirEncontroContas);
            Map(b => b.PermitirCondicional);
            Map(b => b.FinQtdeDiasPagto);
            Map(b => b.FinanceiraPercTaxaParc);
            Map(b => b.Irrestrito);
            Map(b => b.BaixaLimite);
            Map(b => b.ExigeCartao);
            Map(b => b.PermiteAlterarVencimento);
            Map(b => b.PermiteVenderFazendaInativa);
            Map(b => b.MomentoBaixaLimite);
            Map(b => b.BaixarLimiteTodosDocAbe);
            Map(b => b.UtilizadoFrete);
            Map(b => b.PermitirLancamentoManual);
            Map(b => b.BloqueiaPorFaltaDePagamento);
            Map(b => b.ExigePdv);
            Map(b => b.ObrigatorioEncontroContas);
            Map(b => b.EmiteDuplicata);
            Map(b => b.InformaTipoDeClientePermitido);
            Map(b => b.PermiteLancarEmDevolucao);
            Map(b => b.PermiteVencimentoSemanalRateio);
            Map(b => b.VerificaLimite);
            Map(b => b.LocalBaixa);
            Map(b => b.PermiteBaixar);
            Map(b => b.BaixaLimiteVenda);
            Map(b => b.BaixaLimiteCondicional);
            Map(b => b.CondicaoCalculoJuros);
            Map(b => b.ParticipaConvenio);
            Map(b => b.EncontroContasAutomatico);
            Map(b => b.PermiteNumParMaiQueVenCar);
            Map(b => b.ApareceExtratoFinanceiro);
            Map(b => b.PermiteLinkPagamento);
            Map(b => b.AplicacaoCaixa);

            Table("TipoContaReceber");
        }
    }
}
