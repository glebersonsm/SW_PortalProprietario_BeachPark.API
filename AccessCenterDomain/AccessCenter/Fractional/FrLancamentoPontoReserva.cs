namespace AccessCenterDomain.AccessCenter.Fractional
{
    public class FrLancamentoPontoReserva : EntityBase
    {
        public virtual int? GrupoEmpresa { get; set; } = 1;
        public virtual int? Empresa { get; set; } = 1;
        public virtual int? Filial { get; set; } = 1;
        public virtual int? FrLancamentoPonto { get; set; }
        public virtual int? Reserva { get; set; }
        public virtual DateTime? DataHoraCancelamento { get; set; }
        public virtual string Cancelado { get; set; } = "N";
        public virtual int? UsuarioCancelamento { get; set; }
    }
}
