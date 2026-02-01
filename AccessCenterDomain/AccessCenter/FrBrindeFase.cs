namespace AccessCenterDomain.AccessCenter
{
    public class FrBrindeFase : EntityBase
    {
        public virtual int? Filial { get; set; }
        public virtual string? Nome { get; set; }
        public virtual string? NomePesquisa { get; set; }
        public virtual string? PermitirUtilizarAbordagem { get; set; } = "N";
        public virtual string? PermitirUtilizarSala { get; set; } = "N";
        public virtual string? PermitirUtilizarRecepcao { get; set; } = "N";
        public virtual string? PermitirUtilizarNegociacao { get; set; } = "N";
        public virtual string? PermitirUtilizarAvulso { get; set; } = "N";
        public virtual string? Status { get; set; } = "A";

    }
}
