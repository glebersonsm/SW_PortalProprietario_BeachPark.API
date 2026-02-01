namespace AccessCenterDomain.AccessCenter.Fractional
{
    public class FrBrindeFase : EntityBaseEsol
    {

        public int? Filial { get; set; }
        public string? Nome { get; set; }
        public string? NomePesquisa { get; set; }
        public string? PermitirUtilizarAbordagem { get; set; } = "N";
        public string? PermitirUtilizarSala { get; set; } = "N";
        public string? PermitirUtilizarRecepcao { get; set; } = "N";
        public string? PermitirUtilizarNegociacao { get; set; } = "N";
        public string? PermitirUtilizarAvulso { get; set; } = "N";
        public string? Status { get; set; } = "A";

    }
}
