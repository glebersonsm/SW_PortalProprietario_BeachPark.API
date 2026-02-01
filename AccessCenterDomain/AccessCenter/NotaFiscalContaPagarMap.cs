using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class NotaFiscalContaPagarMap : ClassMap<NotaFiscalContaPagar>
    {
        public NotaFiscalContaPagarMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("NOTAFISCALCONTAPAGAR_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(b => b.NotaFiscal);
            Map(b => b.ContaPagar);
            Map(b => b.LancaContasAPagar);
            Map(b => b.LancamentoOriginal);
            Map(b => b.AlteradorValorPis);
            Map(b => b.AlteradorValorCofins);
            Map(b => b.AlteradorValorInss);
            Map(b => b.AlteradorValorIss);
            Map(b => b.AlteradorValorIrrf);
            Map(b => b.AlteradorValorContribuicao);

            Table("NotaFiscalContaPagar");
        }
    }
}
