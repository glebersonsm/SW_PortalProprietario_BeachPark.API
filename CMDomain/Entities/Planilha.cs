namespace CMDomain.Entities
{
    public class Planilha : CMEntityBase
    {
        public virtual int? PlnCodigo { get; set; }
        public virtual int? IdModulo { get; set; } = 3;
        public virtual int? PanCodigo { get; set; }
        public virtual int? PerNumero { get; set; } = DateTime.Now.Month;
        public virtual int? PerExercicio { get; set; } = DateTime.Now.Year;
        public virtual DateTime? PlnDatDia { get; set; }
        public virtual int? PlnPlanil { get; set; }
        public virtual int? PlnNumLan { get; set; } = 2;
        public virtual string? TipCodigo { get; set; } = "03";
        public virtual decimal? PlnTotDebOficial { get; set; } = 0.00m;
        public virtual decimal? PlnTotCreOficial { get; set; } = 0.00m;
        public virtual decimal? PlnTotDebHist { get; set; } = 0.00m;
        public virtual decimal? PlnTotCreHist { get; set; } = 0.00m;
        public virtual decimal? PlnTotDebGer { get; set; } = 0.00m;
        public virtual decimal? PlnTotCreGer { get; set; } = 0.00m;
        public virtual decimal? PlnTotDebGeren1 { get; set; } = 0.00m;
        public virtual decimal? PlnTotCreGeren1 { get; set; } = 0.00m;
        public virtual decimal? PlnTotDebGeren2 { get; set; } = 0.00m;
        public virtual decimal? PlnTotCreGeren2 { get; set; } = 0.00m;
        public virtual decimal? PlnTotDeb { get; set; } = 0.00m;
        public virtual decimal? PlnTotCre { get; set; } = 0.00m;
        public virtual DateTime? DtExtemporaneo { get; set; }
        public virtual string? PlnRefOutroSis { get; set; } = "N";
        public virtual string? PlnEfetivado { get; set; } = "N";
        public virtual int? IdUsuarioInclusao { get; set; }
        public virtual int? IdPessoa { get; set; }
        public virtual int? PlnEmUso { get; set; } = 0;
        public virtual int? LoteTransmissao { get; set; }
        public virtual int? PlnPlanEstorno { get; set; }
        public virtual int? PlnReferencia { get; set; }
        public virtual string? FlgEncerramento { get; set; } = "N";
        public virtual int? IdEmpresaOrigem { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }

    }
}
