using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class ValorAgregCotGMap : ClassMap<ValorAgregCotG>
    {
        public ValorAgregCotGMap()
        {
            Id(p => p.IdValorAgregCotG).GeneratedBy.Sequence("SEQVALORAGREGCOTG"); ;

            Map(p => p.CodProcesso);
            Map(p => p.IdForCli);
            Map(p => p.Proposta);
            Map(p => p.Valor);
            Map(p => p.Percentual);
            Map(p => p.CodTipoCustAgreg);
            Map(p => p.TrgDtInclusao);
            Map(p => p.TrgUserInclusao);

            Table("ValorAgregCotG");
        }
    }
}
