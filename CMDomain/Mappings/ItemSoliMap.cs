using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class ItemSoliMap : ClassMap<ItemSoli>
    {

        public ItemSoliMap()
        {
            Id(x => x.IdItemSoli).GeneratedBy.Assigned();

            Map(p => p.NumSolCompra);
            Map(p => p.CodArtigo);
            Map(p => p.IdProcXArt);
            Map(p => p.ObsItemSolic);
            Map(p => p.CodMedida);
            Map(p => p.CodProcesso).Nullable();
            Map(p => p.IdContratoProd).Nullable();
            Map(p => p.IdProdVari);
            Map(p => p.QtdePedida);
            Map(p => p.SaldoAComprar);
            Map(p => p.QtdePendente);
            Map(p => p.SoliciAceita);
            Map(p => p.IdComprador);
            Map(p => p.DataCancel);
            Map(p => p.StatusItem);
            Map(p => p.IdUsuarioCancel);
            Map(p => p.TrgDtInclusao);
            Map(p => p.TrgUserInclusao);

            Schema("cm");
            Table("ItemSoli");
        }
    }
}
