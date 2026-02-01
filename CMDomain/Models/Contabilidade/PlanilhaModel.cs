namespace CMDomain.Models.Contabilidade
{
    public class PlanilhaModel
    {
        public int? PlnCodigo { get; set; }
        public int? idModulo { get; set; }
        public int? PerNumero { get; set; }
        public DateTime? PlnDatDia { get; set; }
        public int? PlnPlanil { get; set; }
        public int? PlnNumLan { get; set; }
        public decimal? PlnTotDeb { get; set; }
        public decimal? PlnTotCre { get; set; }
        public string? PlnEfetivado { get; set; }
        public int? IdUsuarioInclusao { get; set; }
        public string? TipCodigo { get; set; }
        public int? IdPessoa { get; set; }
        public string? TrgUserInclusao { get; set; }
        public DateTime? TrgDtInclusao { get; set; }

    }
}
