using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class LfModeloMap : ClassMap<LfModelo>
    {
        public LfModeloMap()
        {
            Id(x => x.IdModelo)
                .GeneratedBy.Assigned();

            Map(p => p.Descricao);
            Map(p => p.Modelo);
            Map(p => p.ModeloSped);
            Map(p => p.FlgConsideraDtSaida);
            Map(p => p.Sigla);
            Map(p => p.FlgTipo);
            Map(p => p.TrgDtInclusao);
            Map(p => p.TrgUserInclusao);

            Table("LfModelo");
        }
    }
}
