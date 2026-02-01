using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class NotaFiscalContaPagDesRecPagMap : ClassMap<NotaFiscalContaPagDesRecPag>
    {
        public NotaFiscalContaPagDesRecPagMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("NOTAFISCALCONTAPAGDESRECPAG_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(b => b.NotaFiscal);
            Map(b => b.ContaPagar);
            Map(b => b.Filial);
            Map(b => b.DestinoContabil);
            Map(b => b.CentroCusto);
            Map(b => b.AtividadeProjeto);
            Map(b => b.Historico);
            Map(b => b.HistoricoContabil);
            Map(b => b.Observacao);
            Map(b => b.Valor);

            Table("NotaFiscalContaPagDesRecPag");
        }
    }
}
