using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class AlmoxMap : ClassMap<Almox>
    {
        public AlmoxMap()
        {
            Id(x => x.CodAlmoxarifado)
            .GeneratedBy.Assigned();

            Map(p => p.CodCusteio);

            Map(b => b.IdPessoa);

            Map(b => b.CodCentroCusto);

            Map(p => p.IdEmpresa)
                .Nullable();

            Map(b => b.DescAlmox)
                .Nullable();

            Map(b => b.PrincipSecund);

            Map(b => b.Contabil)
                .Nullable();

            Map(b => b.TrgDtInclusao);
            Map(b => b.TrgUserInclusao);

            Schema("cm");
            Table("Almox");
        }
    }
}
