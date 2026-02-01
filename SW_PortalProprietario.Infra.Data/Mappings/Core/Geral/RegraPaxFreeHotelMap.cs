using FluentNHibernate.Mapping;
using SW_PortalProprietario.Domain.Entities.Core.Geral;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.Geral
{
    public class RegraPaxFreeHotelMap : ClassMap<RegraPaxFreeHotel>
    {
        public RegraPaxFreeHotelMap()
        {
            Id(x => x.Id).GeneratedBy.Native("RegraPaxFreeHotel_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.DataHoraAlteracao).Nullable();
            Map(p => p.UsuarioAlteracao).Nullable();
            Map(p => p.UsuarioRemocao).Nullable();
            Map(p => p.DataHoraRemocao).Nullable();

            References(b => b.RegraPaxFree, "RegraPaxFree");
            Map(b => b.HotelId).Nullable();

            Table("RegraPaxFreeHotel");
        }
    }
}

