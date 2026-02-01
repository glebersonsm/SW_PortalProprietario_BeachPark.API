using FluentNHibernate.Mapping;

namespace AccessCenterDomain.AccessCenter
{
    public class MovimentacaoFinanceiraSalDiaMap : ClassMap<MovimentacaoFinanceiraSalDia>
    {
        public MovimentacaoFinanceiraSalDiaMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("MOVIMENTACAOFINANCEIRASALDIA_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(b => b.Empresa);
            Map(b => b.GrupoEmpresa);
            Map(b => b.Data);
            Map(b => b.TotalDebito);
            Map(b => b.TotalCredito);
            Map(b => b.TotalSaldo);
            Map(b => b.ContaFinanceiraVariacao);
            Map(b => b.ContaFinanceiraSubVariacao);

            Table("MovimentacaoFinanceiraSalDia");
        }
    }
}
