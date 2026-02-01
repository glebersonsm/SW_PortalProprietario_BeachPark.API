using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class CentCustMap : ClassMap<CentCust>
    {
        public CentCustMap()
        {
            CompositeId()
                .KeyProperty(x => x.CodCentroCusto)
                .KeyProperty(x => x.IdEmpresa);

            Map(p => p.Nome);
            Map(p => p.Ativo);
            Map(p => p.StatusGrupoCDC);
            Map(p => p.CodExterno);
            Map(p => p.TrgUserInclusao);
            Map(p => p.TrgDtInclusao);

            Table("CentCust");
        }
    }
}
