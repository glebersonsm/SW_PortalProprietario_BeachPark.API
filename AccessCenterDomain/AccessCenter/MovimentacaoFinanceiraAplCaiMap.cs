using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class MovimentacaoFinanceiraAplCaiMap : ClassMap<MovimentacaoFinanceiraAplCai>
    {
        public MovimentacaoFinanceiraAplCaiMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("MOVIMENTACAOFINANCEIRAAPLCAI_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(b => b.MovimentacaoFinanceira);
            Map(b => b.AplicacaoCaixa);
            Map(b => b.Valor);

            Table("MovimentacaoFinanceiraAplCai");
        }
    }
}
