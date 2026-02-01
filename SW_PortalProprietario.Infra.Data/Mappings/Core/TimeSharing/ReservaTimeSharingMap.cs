using FluentNHibernate.Mapping;
using NHibernate.Type;
using SW_PortalProprietario.Domain.Entities.Core.Geral;
using SW_PortalProprietario.Domain.Entities.Core.Sistema;
using SW_PortalProprietario.Domain.Entities.Core.TimeSharing;
using SW_PortalProprietario.Domain.Enumns;
using SW_PortalProprietario.Infra.Data.Mappings.Core.Sistema;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.TimeSharing
{
    public class ReservaTimeSharingMap : ClassMap<ReservaTimeSharing>
    {
        public ReservaTimeSharingMap()
        {
            Id(x => x.Id).GeneratedBy.Native("ReservaTimeSharing_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);

            Map(b => b.DataHoraCriacao);

            Map(b => b.DataHoraAlteracao)
                .Nullable();

            Map(p => p.UsuarioAlteracao)
                .Nullable();

            Map(p => p.IdVendaXContrato);
            Map(p => p.ClienteReservante);
            Map(p => p.IdReservasFront);
            Map(p => p.PontosUtilizados);
            Map(p => p.FracionamentoIdCriado);
            Map(p => p.FracionamentoIdFinalizado);
            Map(p => p.Checkin);
            Map(p => p.Checkout);
            Map(p => p.Adultos);
            Map(p => p.Criancas1);
            Map(p => p.Criancas2);
            Map(p => p.IdTipoUh);
            Map(p => p.TipoUtilizacao);
            Map(p => p.StatusCM);
            Map(p => p.NomeCliente);
            Map(p => p.UsuarioVinculacao);
            Map(p => p.NumReserva);
            Map(p => p.NumeroContrato);
            Map(p => p.MotivoCancelamentoInfUsu).Length(2000);
            Map(p => p.ClienteNotificadoCancelamento).CustomType<EnumType<EnumSimNao>>();

            Table("ReservaTimeSharing");
        }
    }
}
