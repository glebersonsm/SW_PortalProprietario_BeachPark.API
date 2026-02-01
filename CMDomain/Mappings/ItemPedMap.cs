using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class ItemPediMap : ClassMap<ItemPedi>
    {
        public ItemPediMap()
        {
            CompositeId()
                .KeyProperty(x => x.NumRequisicao)
                .KeyProperty(x => x.CodArtigo);

            Map(b => b.CodMedida);

            Map(b => b.ValorUn);

            Map(p => p.QtdePedida);

            Map(b => b.QtdePendente);

            Map(b => b.FlgSci);

            Map(b => b.Obs);
            Map(b => b.QtdePendVenda);
            Map(b => b.DtCancelamento);
            Map(b => b.QtdeCancelada);
            Map(b => b.QtdeAprovadaRad);
            Map(b => b.DataNecessidade);

            Map(b => b.TrgDtInclusao);
            Map(b => b.TrgUserInclusao);

            Table("ItemPedi");
        }
    }
}
