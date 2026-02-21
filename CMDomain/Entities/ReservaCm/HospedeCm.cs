using System;

namespace CMDomain.Entities.ReservaCm;

public class HospedeCm
{
    public virtual long IdHospede { get; set; }
    public virtual long? IdCidade { get; set; }
    public virtual long? IdIdioma { get; set; }
    public virtual long? IdFaixaEtaria { get; set; }
    public virtual string? SobreNome { get; set; }
    public virtual string? Nome { get; set; }
    public virtual string? RecebeMalaDireta { get; set; }
    public virtual DateTime? DataNascimento { get; set; }
    public virtual long? TipoEtario { get; set; }
    public virtual string Fumante { get; set; } = "N";
    public virtual string? UsuarioInclusao { get; set; }
    public virtual string? UsuarioAlteracao { get; set; }
    public virtual string BloqueiaLancamentoManual { get; set; } = "N";
    public virtual long? IdTipoHospede { get; set; }
}
