namespace AccessCenterDomain.AccessCenter.Fractional
{
    public class FrPessoa : EntityBase
    {
        public virtual int? Filial { get; set; }
        public virtual int? Pessoa { get; set; }
        public virtual int? TempoCasamento { get; set; }
        public virtual int? QuantidadeDependentes { get; set; } = 0;
        public virtual int? QuantidadeFilhos { get; set; } = 0;
        public virtual string? IdadeFilhos { get; set; }
        public virtual string? PossuiCartaoCredito { get; set; } = "S";
        public virtual string? PossuiCarro { get; set; } = "S";
        public virtual string? ResidenciaPropria { get; set; } = "S";
        public virtual string? UtilizouOcr { get; set; } = "N";
        public virtual decimal? Renda { get; set; } = 0;
    }
}
