using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class UsuXRelXEmpMap : ClassMap<UsuXRelXEmp>
    {
        public UsuXRelXEmpMap()
        {
            CompositeId()
                .KeyProperty(x => x.IdEmpresa)
                .KeyProperty(x => x.IdReports)
                .KeyProperty(x => x.OrigemCM)
                .KeyProperty(x => x.IdEspAcesso);

            Map(b => b.FlgHabilita);
            Map(b => b.FlgVisualiza);
            Map(b => b.TrgDtInclusao);
            Map(b => b.TrgUserInclusao);

            Table("UsuXRelXEmp");
        }
    }
}
