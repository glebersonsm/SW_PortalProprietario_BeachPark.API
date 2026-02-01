using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class TipoAlteradorMap : ClassMap<TipoAlterador>
    {
        public TipoAlteradorMap()
        {
            Id(x => x.CodAlterador).GeneratedBy.Assigned();

            Map(p => p.IdPessoa);
            Map(b => b.IdEmpresa);
            Map(b => b.Plano);
            Map(p => p.Descricao);
            Map(p => p.AcresDecres);
            Map(p => p.FlgStatus);
            Map(p => p.RecPag);
            Map(p => p.PlaConta);
            Map(b => b.TrgDtInclusao);
            Map(b => b.TrgUserInclusao);

            Table("TipoAlterador");
        }
    }
}
