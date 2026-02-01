using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class CentroCustoUsuarioMap : ClassMap<CentroCustoUsuario>
    {
        public CentroCustoUsuarioMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("CENTROCUSTOUSUARIO_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);


            Map(b => b.Usuario);
            Map(b => b.CentroCusto);

            Table("CentroCustoUsuario");
        }
    }
}
