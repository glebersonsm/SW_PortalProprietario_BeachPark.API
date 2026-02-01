using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class CentroCustoEmpresaMap : ClassMap<CentroCustoEmpresa>
    {
        public CentroCustoEmpresaMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("CENTROCUSTOEMPRESA_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);


            Map(b => b.GrupoEmpresa);
            Map(b => b.CentroCusto);
            Map(p => p.Empresa);

            Table("CentroCustoEmpresa");
        }
    }
}
