using CMDomain.Entities.ReservaCm;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings.ReservaCm;

public class ReservaReduzidaCmMap : ClassMap<ReservaReduzidaCm>
{
    public ReservaReduzidaCmMap()
    {
        Table("RESERVAREDUZ");
        Schema("cm");

        Id(x => x.IdReserva, "idreservasfront").GeneratedBy.Assigned();

        Map(x => x.IdHotel, "idhotel");
        Map(x => x.DataCheckinPrevisto, "datachegada");
        Map(x => x.DataCheckoutPrevisto, "datapartida");
        Map(x => x.StatusReserva, "statusreserva");
        Map(x => x.IdFornecedorCliente, "idforcli");
        Map(x => x.CodigoContrato, "codcontrato");
        Map(x => x.TipoUh, "idtipouh");
        Map(x => x.TrgDataInclusao, "trgdtinclusao");
        Map(x => x.UsuarioInclusao, "trguserinclusao").ReadOnly();
    }
}
