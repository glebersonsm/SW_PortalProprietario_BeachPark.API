using FluentNHibernate.Mapping;
using SW_PortalProprietario.Domain.Entities.Core.Geral;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.Geral
{
    public class ConfigReservaVhfMap : ClassMap<ConfigReservaVhf>
    {
        public ConfigReservaVhfMap()
        {
            Id(x => x.Id).GeneratedBy.Native("ConfigReservaVhf_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.DataHoraAlteracao).Nullable();
            Map(p => p.UsuarioAlteracao).Nullable();

            Map(b => b.TipoUtilizacao).Length(4000).Not.Nullable();
            Map(b => b.TipoNegocio).Length(4000).Nullable();
            Map(b => b.HotelId).Nullable();
            Map(b => b.TipoHospede).Length(2000).Not.Nullable();
            Map(b => b.TipoHospedeCrianca1).Length(2000).Nullable();
            Map(b => b.TipoHospedeCrianca2).Length(2000).Nullable();
            Map(b => b.Origem).Length(4000).Not.Nullable();
            Map(b => b.TarifaHotel).Length(4000).Not.Nullable();
            Map(b => b.EncaixarSemanaSeHouver).Not.Nullable();
            Map(b => b.Segmento).Length(4000).Nullable();
            Map(b => b.CodigoPensao).Length(2000).Not.Nullable();
            Map(b => b.PermiteIntercambioMultipropriedade).Not.Nullable();

            Schema("portalohana");
            Table("ConfigReservaVhf");
        }
    }
}
