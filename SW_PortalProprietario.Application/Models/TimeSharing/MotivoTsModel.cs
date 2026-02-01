namespace SW_PortalProprietario.Application.Models.TimeSharing
{
    public class MotivoTsModel
    {
        public virtual int? IdMotivoTs { get; set; }
        public virtual string? CodReduzido { get; set; }
        public virtual string? Descricao { get; set; }
        public virtual string? Aplicacao { get; set; }
        public virtual string? FlgAtivo { get; set; }
        public virtual string? FlgLancFuturo { get; set; }
        public virtual string? FlgContaUtilizacao { get; set; }
        public virtual int? IdTipoDcCred { get; set; }
        public virtual int? IdHotel { get; set; }
        public virtual int? IdTipoDcDeb { get; set; }
        public virtual string? FlgPermlancCredito { get; set; }
        public virtual int? IdDepartamento { get; set; }
        public virtual string? Observacao { get; set; }
        public virtual string? FlgGeraComissao { get; set; }
        public virtual string? FlgBloqueiaComissao { get; set; }
        public virtual string? FlgNegociacao { get; set; }
        public virtual int? IdTipoIndice { get; set; }
        public virtual string? FlgEncerraAutoOcorrencia { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }
    }
}
