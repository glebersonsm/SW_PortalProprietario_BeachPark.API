using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class EmpresaMap : ClassMap<Empresa>
    {
        public EmpresaMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("EMPRESA_SEQUENCE");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(b => b.Codigo);
            Map(p => p.Pessoa);
            Map(p => p.GrupoEmpresa);
            Map(p => p.Status);

            Table("Empresa");
        }
    }
}
