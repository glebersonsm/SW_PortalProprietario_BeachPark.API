using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class GrupProdMap : ClassMap<GrupProd>
    {
        public GrupProdMap()
        {
            Id(x => x.CodGrupoProd).GeneratedBy.Assigned();

            Map(p => p.DescGrupoProd);

            Map(b => b.StatusGrupo);

            Map(b => b.FlgIndicaServico);
            Map(b => b.IdPessoa);
            References(b => b.NaturezaEstoque, "IdNaturezaEstoque");
            Map(b => b.FlgGrupoFixo);
            Map(b => b.TipoEstoque);

            Map(b => b.CodigoNCM);

            Map(b => b.TrgDtInclusao);
            Map(b => b.TrgUserInclusao);

            Schema("cm");
            Table("GrupProd");
        }
    }
}
