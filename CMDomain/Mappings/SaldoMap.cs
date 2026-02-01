using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class SaldoMap : ClassMap<Saldo>
    {
        public SaldoMap()
        {
            CompositeId()
                .KeyProperty(x => x.CodArtigo)
                .KeyProperty(x => x.CodAlmoxarifado);

            Map(b => b.SaldoQtde);

            Map(b => b.IdPessoa);

            Map(b => b.TrgDtInclusao);
            Map(b => b.TrgUserInclusao);

            Table("Saldo");
        }
    }
}
