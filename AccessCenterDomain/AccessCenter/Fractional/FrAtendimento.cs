namespace AccessCenterDomain.AccessCenter.Fractional
{
    public class FrAtendimento : EntityBase
    {
        public virtual int? Filial { get; set; }
        public virtual int? LgCampanha { get; set; }
        public virtual int? LgPontoCaptacao { get; set; }
        public virtual int? LgQuestionario { get; set; }
        public virtual int? FrSala { get; set; }
        public virtual string Status { get; set; } = "F";
        public virtual DateTime? DataHoraFinalizacao { get; set; }
        public virtual DateTime? DataHoraAtendimento { get; set; }
        public virtual int? UsuarioFinalizacao { get; set; }
        public virtual int? FrQualificacao { get; set; } = 1;
        public virtual string TipoQualificacao { get; set; } = "M";
        public virtual int? GrupoEmpresa { get; set; }
        public virtual int? Empresa { get; set; }
        public virtual int? FrQualificacaoAutomatica { get; set; }
        public virtual int? FrPessoa1 { get; set; }
        public virtual int? FrPessoa2 { get; set; }
        public virtual string Fase { get; set; } = "S";
        public virtual string FaseStatus { get; set; } = "H";
        public virtual string FTB { get; set; } = "N";
        public virtual int? IdPromotorTlmkt { get; set; }
        public virtual int? IdPromotor { get; set; }
        public virtual int? IdLiner { get; set; }
        public virtual int? IdCloser { get; set; }
        public virtual int? IdPep { get; set; }
        public virtual int? IdFtbSugerido { get; set; }
        public virtual int? IdLinerSugerido { get; set; }
        public virtual int? IdCloserSugerido { get; set; }
        public virtual int? IdPepSugerido { get; set; }
        public virtual int? FlgFtb { get; set; }

    }
}
