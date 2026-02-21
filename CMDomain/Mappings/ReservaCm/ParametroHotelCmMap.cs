using CMDomain.Entities.ReservaCm;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings.ReservaCm;

public class ParametroHotelCmMap : ClassMap<ParametroHotelCm>
{
    public ParametroHotelCmMap()
    {
        Table("paramhotel");
        Schema("cm");

        Id(x => x.IdHotel, "idhotel").GeneratedBy.Assigned();

        Map(x => x.HoraCheckIn, "horacheckin");
        Map(x => x.HoraChekOut, "horacheckout");
        Map(x => x.IdadeMaximaCrianca1, "idademaxcri1");
        Map(x => x.IdadeMaximaCrianca2, "idademaxcri2");
    }
}
