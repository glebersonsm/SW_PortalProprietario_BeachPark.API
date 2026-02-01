using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class ClienteTipoClienteMap : ClassMap<ClienteTipoCliente>
    {
        public ClienteTipoClienteMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("CLIENTETIPOCLIENTE_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);


            Map(b => b.TipoCliente);
            Map(b => b.Cliente);
            Map(b => b.DataHoraRemocao);

            Table("ClienteTipoCliente");
        }
    }
}
