using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class RecbtoPagtoMap : ClassMap<RecbtoPagto>
    {
        public RecbtoPagtoMap()
        {
            CompositeId()
                .KeyProperty(x => x.CodDocumento)
                .KeyProperty(x => x.NumLancto);

            Map(p => p.IdUsuarioInclusao);
            Map(p => p.CodPortForma);
            Map(p => p.NumChqBordero);
            Map(p => p.DatacFloat);
            Map(p => p.CodLancFinanc);
            Map(p => p.NumLote);
            Map(p => p.DataBaixa);

            Map(p => p.TrgDtInclusao);
            Map(p => p.TrgUserInclusao);

            Table("RecbtoPagto");
        }
    }
}
