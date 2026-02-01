using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class AplicacaoCaixaMap : ClassMap<AplicacaoCaixa>
    {
        public AplicacaoCaixaMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("CENTROCUSTOFILIAL_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);


            Map(b => b.Codigo);
            Map(b => b.Nome);
            Map(p => p.NomePesquisa);
            Map(p => p.AplicacaoCaixaGrupo);
            Map(p => p.StatusLancamento);
            Map(p => p.StatusConsulta);
            Map(p => p.ContaReceber);
            Map(p => p.ContaPagar);
            Map(p => p.MovimentacaoFinanceiraManual);

            Table("AplicacaoCaixa");
        }
    }
}
