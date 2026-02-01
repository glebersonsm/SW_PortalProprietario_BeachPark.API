using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class NotaFiscalContaPagarRetImpMap : ClassMap<NotaFiscalContaPagarRetImp>
    {
        public NotaFiscalContaPagarRetImpMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("NOTAFISCALCONTAPAGARRETIMP_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(b => b.NotaFiscal);
            Map(b => b.ContaPagar);
            Map(b => b.TipoImpostoRetido);

            Table("NotaFiscalContaPagarRetImp");
        }
    }

}
