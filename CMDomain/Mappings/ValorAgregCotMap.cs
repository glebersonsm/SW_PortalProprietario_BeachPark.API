using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class ValorAgregCotMap : ClassMap<ValorAgregCot>
    {
        public ValorAgregCotMap()
        {
            CompositeId().KeyProperty(p => p.CodProcesso)
                .KeyProperty(p => p.IdProcXArt)
                .KeyProperty(p => p.Proposta)
                .KeyProperty(p => p.CodTipoCustAgreg)
                .KeyProperty(p => p.IdForCli);

            Map(p => p.Valor);
            Map(p => p.Percentual);
            Map(p => p.BaseCalculo);
            Map(p => p.PercBaseCalculo);
            Map(p => p.TrgDtInclusao);
            Map(p => p.TrgUserInclusao);

            Table("ValorAgregCot");
        }
    }
}
