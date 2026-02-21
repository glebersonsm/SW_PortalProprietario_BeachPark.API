using CMDomain.Entities.ReservaCm;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings.ReservaCm;

public class MovimentoHospedeCmMap : ClassMap<MovimentoHospedeCm>
{
    public MovimentoHospedeCmMap()
    {
        Table("movimentohospedes");
        Schema("cm");

        CompositeId()
            .KeyProperty(x => x.IdResevasFront, "idreservasfront")
            .KeyProperty(x => x.IdHospede, "idhospede");

        Map(x => x.IdHotel, "idhotel");
        Map(x => x.DataChekinPrevisto, "datachegprevista");
        Map(x => x.DataCheckinReal, "datachegreal");
        Map(x => x.DataCheckoutPrevisto, "datapartprevista");
        Map(x => x.DataCheckoutReal, "datapartreal");
        Map(x => x.IdTipoHospede, "idtipohospede");
        Map(x => x.Incognito, "incognito");
        Map(x => x.HoraCheckinPrevista, "horachegprevista");
        Map(x => x.HoraCheckoutPrevista, "horapartprevista");
        Map(x => x.UsoDaCasa, "usodacasa");
        Map(x => x.PercentualDiaria, "percdiaria");
        Map(x => x.DiariaConfidencial, "diariaconfidencia");
        Map(x => x.Cofre, "cofre");
        Map(x => x.Principal, "Principal");
        Map(x => x.MenorIdade, "menoridade");
        Map(x => x.IdResponsavel, "idresponsavel");
        Map(x => x.UsuarioInclusao, "trguserinclusao");
        Map(x => x.DataInclusao, "trgdtinclusao");
    }
}
