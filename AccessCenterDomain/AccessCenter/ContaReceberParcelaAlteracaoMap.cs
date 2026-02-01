using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class ContaReceberParcelaAlteracaoMap : ClassMap<ContaReceberParcelaAlteracao>
    {
        public ContaReceberParcelaAlteracaoMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("CONTARECEBERPARCELAALTERACAO_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(p => p.ContaReceberParcela);
            Map(b => b.TipoContaReceber);
            Map(b => b.TipoContaReceberAnterior);
            Map(b => b.ClienteAnterior);
            Map(b => b.Cliente);
            Map(b => b.Valor);
            Map(b => b.Data);
            Map(b => b.Estornado);
            Map(b => b.VencimentoAnterior);
            Map(b => b.NovoVencimento);

            Table("ContaReceberParcelaAlteracao");
        }
    }
}
