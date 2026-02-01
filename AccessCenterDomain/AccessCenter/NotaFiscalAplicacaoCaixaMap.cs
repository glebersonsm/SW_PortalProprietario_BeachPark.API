using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class NotaFiscalAplicacaoCaixaMap : ClassMap<NotaFiscalAplicacaoCaixa>
    {
        public NotaFiscalAplicacaoCaixaMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("NOTAFISCALAPLICACAOCAIXA_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(b => b.NotaFiscal);
            Map(b => b.AplicacaoCaixa);
            Map(b => b.Valor);

            Table("NotaFiscalAplicacaoCaixa");
        }
    }
}
