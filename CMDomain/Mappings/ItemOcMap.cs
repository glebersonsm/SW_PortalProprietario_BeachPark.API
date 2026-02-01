using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class ItemOcMap : ClassMap<ItemOc>
    {
        public ItemOcMap()
        {
            Id(x => x.IdItemOc).GeneratedBy.Sequence("SEQITEMOC");

            Map(p => p.NumOc);
            Map(p => p.CodArtigo);
            Map(p => p.CodMedida);
            Map(p => p.QtdePedida);
            Map(p => p.QtdeRecebida);
            Map(p => p.FlgItemAtendido);
            Map(p => p.ObsItemOc);
            Map(p => p.ValorUn);
            Map(p => p.ValorInicial);
            Map(p => p.CanceladoPor);
            Map(p => p.TrgDtInclusao);
            Map(p => p.TrgUserInclusao);

            Table("ItemOc");
        }
    }
}
