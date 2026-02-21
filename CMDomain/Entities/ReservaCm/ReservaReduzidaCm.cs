using System;

namespace CMDomain.Entities.ReservaCm;

public class ReservaReduzidaCm
{
    public virtual long IdReserva { get; set; }
    public virtual long IdHotel { get; set; }
    public virtual DateTime? DataCheckinPrevisto { get; set; }
    public virtual DateTime? DataCheckoutPrevisto { get; set; }
    public virtual long? StatusReserva { get; set; }
    public virtual long? IdFornecedorCliente { get; set; }
    public virtual long? CodigoContrato { get; set; }
    public virtual long? TipoUh { get; set; }
    public virtual DateTime? TrgDataInclusao { get; set; }
    public virtual string? UsuarioInclusao { get; set; }
}
