using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class ProdutoMap : ClassMap<Produto>
    {
        public ProdutoMap()
        {
            Id(x => x.CodProduto).GeneratedBy.Assigned();

            References(p => p.GrupProd, "CodGrupoProd");

            Map(b => b.CodMedCusto);

            Map(b => b.DescProd);

            Map(p => p.CodMedAnalise);

            Map(b => b.ConsumoRevenda);

            Map(b => b.LoteValidade);

            Map(b => b.ItemEstocavel);
            Map(b => b.DescrCompl);
            Map(b => b.CodMenorMed);
            Map(b => b.FlgVariavel);
            Map(b => b.CodigoNCM);
            Map(b => b.CodGenero);
            Map(b => b.Cest);
            Map(b => b.IdGtin);

            Map(b => b.TrgDtInclusao);
            Map(b => b.TrgUserInclusao);

            Table("Produto");
        }
    }
}
