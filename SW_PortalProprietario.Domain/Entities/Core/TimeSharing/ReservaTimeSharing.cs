using SW_PortalProprietario.Domain.Entities.Core.Framework;
using SW_PortalProprietario.Domain.Enumns;
using SW_PortalProprietario.Domain.Entities.Core.Geral;

namespace SW_PortalProprietario.Domain.Entities.Core.TimeSharing
{
    public class ReservaTimeSharing : EntityBaseCore, IEntityValidateCore
    {
        public virtual int? IdVendaXContrato { get; set; }
        public virtual string? NumeroContrato { get; set; }
        public virtual int? IdReservasFront { get; set; }
        public virtual int? ClienteReservante { get; set; }
        public virtual decimal? PontosUtilizados { get; set; }
        public virtual int? FracionamentoIdCriado { get; set; }
        public virtual int? FracionamentoIdFinalizado { get; set; }
        public virtual DateTime? Checkin { get; set; }
        public virtual DateTime? Checkout { get; set; }
        public virtual int? Adultos { get; set; }
        public virtual int? Criancas1 { get; set; }
        public virtual int? Criancas2 { get; set; }
        public virtual int? IdTipoUh { get; set; }
        public virtual string? TipoUtilizacao { get; set; }
        public virtual string? StatusCM { get; set; }
        public virtual string? NomeCliente { get; set; }
        public virtual int? UsuarioVinculacao { get; set; }
        public virtual string? NumReserva { get; set; }
        public virtual string? MotivoCancelamentoInfUsu { get; set; }
        public virtual EnumSimNao? ClienteNotificadoCancelamento { get; set; }
        public virtual async Task SaveValidate()
        {
            await Task.CompletedTask;
        }
    }
}
