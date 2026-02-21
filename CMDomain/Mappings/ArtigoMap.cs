using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class ArtigoMap : ClassMap<Artigo>
    {
        public ArtigoMap()
        {
            Id(x => x.CodArtigo)
                .GeneratedBy.Assigned();

            Map(p => p.CodProduto);

            Map(b => b.CodTipoArtigo);

            Map(b => b.FlgBloqueado);

            Map(p => p.ValUltCompra);

            Map(b => b.FlgAtivo);

            Map(b => b.CodBarra);

            Map(b => b.DescArtigo);
            Map(b => b.CodEan);

            Map(b => b.TrgDtInclusao);
            Map(b => b.TrgUserInclusao);

            Schema("cm");
            Table("Artigo");
        }
    }
}
