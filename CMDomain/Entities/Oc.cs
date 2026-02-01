namespace CMDomain.Entities
{
    public class Oc : CMEntityBase
    {
        public virtual int? NumOc { get; set; }
        public virtual int? IdPessoa { get; set; }
        public virtual int? IdForCli { get; set; }
        public virtual string? OcAtendida { get; set; } = "F";
        public virtual string? FlgImpressa { get; set; } = "F";
        public virtual string? FlgComSemOc { get; set; } = "S";
        public virtual string? ObsOc { get; set; }
        public virtual string? FlgComSemCot { get; set; } = "S";
        public virtual string? FlgRegularizaCap { get; set; } = "S";
        public virtual string? Contato { get; set; }
        public virtual int? FlgTipoFrete { get; set; }
        public virtual int? IdComprador { get; set; }
        public virtual DateTime? DataOc { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }
        public virtual string? FlgMail { get; set; }
        public virtual int? IdEmpCompradora { get; set; }
        public virtual string? FlgDataVencFrete { get; set; } = "EN";
        public virtual string? FlgDataVenc { get; set; } = "EN";
    }
}
