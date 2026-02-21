using System;

namespace CMDomain.Entities.ReservaCm;

public class MovimentoHospedeCm
{
    public virtual long IdResevasFront { get; set; }
    public virtual long IdHospede { get; set; }
    public virtual long? IdHotel { get; set; }
    public virtual DateTime? DataChekinPrevisto { get; set; }
    public virtual DateTime? DataCheckinReal { get; set; }
    public virtual DateTime? DataCheckoutPrevisto { get; set; }
    public virtual DateTime? DataCheckoutReal { get; set; }
    public virtual long? IdTipoHospede { get; set; }
    public virtual string Incognito { get; set; } = "N";
    public virtual DateTime? HoraCheckinPrevista { get; set; }
    public virtual DateTime? HoraCheckoutPrevista { get; set; }
    public virtual string UsoDaCasa { get; set; } = "N";
    public virtual decimal PercentualDiaria { get; set; } = 0;
    public virtual string DiariaConfidencial { get; set; } = "N";
    public virtual string Cofre { get; set; } = "N";
    public virtual string Principal { get; set; } = "N";
    public virtual string MenorIdade { get; set; } = "N";
    public virtual long? IdResponsavel { get; set; }
    public virtual string UsuarioInclusao { get; set; } = string.Empty;
    public virtual DateTime? DataInclusao { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;
        var other = (MovimentoHospedeCm)obj;
        return IdResevasFront == other.IdResevasFront && IdHospede == other.IdHospede;
    }

    public override int GetHashCode() => HashCode.Combine(IdResevasFront, IdHospede);
}
