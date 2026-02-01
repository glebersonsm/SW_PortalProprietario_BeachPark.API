using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class ClienteCartaoCreditoMap : ClassMap<ClienteCartaoCredito>
    {
        public ClienteCartaoCreditoMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("CLIENTECARTAOCREDITO_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(p => p.Cliente);
            Map(b => b.Numero);
            Map(b => b.VencimentoAno);
            Map(b => b.VencimentoMes);
            Map(b => b.Nome);
            Map(b => b.NomePesquisa);
            Map(b => b.CodigoSeguranca);
            Map(b => b.Status);
            Map(b => b.CpfTitular);
            Map(b => b.Financeira);
            Map(b => b.Bandeira);
            Map(b => b.UltimosDigitos);

            Table("ClienteCartaoCredito");
        }
    }
}
