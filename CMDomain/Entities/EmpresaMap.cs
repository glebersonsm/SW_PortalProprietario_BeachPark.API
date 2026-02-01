using FluentNHibernate.Mapping;

namespace CMDomain.Entities
{
    public class EmpresaMap : ClassMap<Empresa>
    {
        public EmpresaMap()
        {
            Id(x => x.IdEmpresa).GeneratedBy.Assigned();

            Map(p => p.NomeEmpresa);

            Table("Empresa");
        }
    }
}
