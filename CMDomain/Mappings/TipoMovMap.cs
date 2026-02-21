using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class TipoMovMap : ClassMap<TipoMov>
    {
        public TipoMovMap()
        {
            Id(x => x.CodTipoMov).GeneratedBy.Assigned();

            Map(p => p.ConsumoMov);

            Map(b => b.DescTipoMov);

            Map(b => b.DescResumida);

            Map(p => p.EntradaSaida);

            Map(b => b.TrgDtInclusao);
            Map(b => b.TrgUserInclusao);

            Table("TipoMov");
            Schema("cm");
        }
    }
}
