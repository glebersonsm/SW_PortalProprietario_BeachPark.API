using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class ItemEntrMap : ClassMap<ItemEntr>
    {
        public ItemEntrMap()
        {
            Id(x => x.IdItemEntrega)
                .GeneratedBy.Assigned();

            Map(p => p.CodArtigo);
            Map(p => p.NumRequisicao);
            Map(p => p.QtdeEntrega);
            Map(p => p.CodMedida);
            Map(p => p.ValorUn);
            Map(p => p.FlgStatus);
            Map(p => p.IdAtendente);
            Map(p => p.IdUsuarioConfDev).Nullable();
            Map(p => p.ObsDevol).Nullable();
            Map(p => p.DataConfDevol).Nullable();
            Map(p => p.FlgConfDevol).Nullable();
            Map(p => p.IdMovDevol).Nullable();
            Map(p => p.IdMov).Nullable();
            Map(p => p.DataEntrega).Nullable();
            Map(p => p.DataReceb).Nullable();
            Map(p => p.TrgDtInclusao).Nullable();
            Map(p => p.TrgUserInclusao).Nullable();

            Schema("cm");
            Table("ItemEntr");
        }
    }
}
