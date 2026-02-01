namespace EsolutionPortalDomain.Portal
{
    public class Cliente : EntityBasePortal
    {
        public virtual string? Situacao { get; set; } = "A";
        public virtual DateTime? DataHoraCadastro { get; set; }
        public virtual DateTime? DataHoraModificacao { get; set; }
        public virtual string? PossuiLimiteCredito { get; set; }
        public virtual decimal? ValorCredito { get; set; }
        public virtual string? PossuiDescontoEspecial { get; set; }
        public virtual decimal? DescontoEspecial { get; set; }
        public virtual int? Pessoa { get; set; }
        public virtual int? Empresa { get; set; }
        public virtual int? IntegracaoStatus { get; set; } = 1;
        public virtual int? IntegracaoTotalTentativa { get; set; } = 0;

    }
}
