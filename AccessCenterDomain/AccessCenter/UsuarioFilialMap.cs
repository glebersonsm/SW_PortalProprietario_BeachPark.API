using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class UsuarioFilialMap : ClassMap<UsuarioFilial>
    {
        public UsuarioFilialMap()
        {
            Id(x => x.Id).GeneratedBy.Sequence("USUARIOFILIAL_");
            Map(b => b.Tag);
            Map(b => b.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.UsuarioAlteracao);
            Map(b => b.DataHoraAlteracao);

            Map(b => b.Usuario);
            Map(b => b.Filial);

            Table("UsuarioFilial");
        }
    }
}
