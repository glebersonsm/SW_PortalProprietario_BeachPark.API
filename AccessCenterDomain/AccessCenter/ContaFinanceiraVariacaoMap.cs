using FluentNHibernate.Mapping;

namespace AccessCenterDomain.AccessCenter
{
    public class ContaFinanceiraVariacaoMap : ClassMap<ContaFinanceiraVariacao>
    {
        public ContaFinanceiraVariacaoMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("CONTAFINANCEIRAVAR_SEQUENCE");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(p => p.ContaFinanceira);
            Map(b => b.Codigo);
            Map(b => b.Nome);
            Map(b => b.NomePesquisa);
            Map(b => b.ContaFinanceiraVariacaoTipo);
            Map(b => b.Principal);
            Map(b => b.ConvenioIntegracaoContaPagar);
            Map(b => b.ExigeSubVariacao);
            Map(b => b.UtilizaSubVariacaoPorUsuario);
            Map(b => b.Status);

            Table("ContaFinanceiraVariacao");
        }
    }
}
