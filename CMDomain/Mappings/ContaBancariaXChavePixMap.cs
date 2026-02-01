using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class ContaBancariaXChavePixMap : ClassMap<ContaBancariaXChavePix>
    {
        public ContaBancariaXChavePixMap()
        {
            Id(x => x.IdContaXChave)
             .GeneratedBy.Sequence("CM.SEQCONTABANCARIAXCHAVEPIX");

            Map(p => p.IdCBancaria);
            Map(p => p.ChavePix);
            Map(p => p.FlgChavePref);
            Map(p => p.IdTipoChave);

            Table("ContaBancariaXChavePix");
        }
    }
}
