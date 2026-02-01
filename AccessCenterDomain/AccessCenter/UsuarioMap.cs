using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class UsuarioMap : ClassMap<Usuario>
    {
        public UsuarioMap()
        {
            Id(x => x.Id).GeneratedBy.Sequence("USUARIO_SEQUENCE");
            Map(b => b.Tag);
            Map(b => b.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.UsuarioAlteracao);
            Map(b => b.DataHoraAlteracao);

            Map(b => b.Codigo);
            Map(b => b.Login);
            Map(b => b.Pessoa);
            Map(b => b.CentroCusto);
            Map(b => b.Status);
            Map(b => b.UsuarioFramework);
            Map(b => b.AlterarSenhaProximoLogon);
            Map(b => b.Senha);

            Table("Usuario");
        }
    }
}
