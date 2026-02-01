namespace AccessCenterDomain.AccessCenter.Model
{
    public class CotaModel
    {
        public int CotaId { get; set; }
        public string? Produto { get; set; }
        public string? Numero { get; set; }
        public string? TagCota { get; set; } //<Importado=S>|<PORTAL=>
        public int? GrupoCotaTipoCota { get; set; }
        public int? Imovel { get; set; }
        public string? ImovelNumero { get; set; }
        public string? TagImovel { get; set; }
        public int? ImovelBloco { get; set; }
        public string? CodigoBloco { get; set; }
        public string? NomeBloco { get; set; }
        public int? Proprietario { get; set; }
        public int? Procurador { get; set; }
        public string? Status { get; set; } = "D";
        public int? FrAtendimentoVenda { get; set; }
        public int? CategoriaCota { get; set; }
        public string? CodigoCategoriaCota { get; set; }
        public string? NomeCategoriaCota { get; set; }
        public string? GrupoCotaTipoCotaCodigo { get; set; }
        public string? GrupoCotaTipoCotaNome { get; set; }
        public int? TipoCota { get; set; }
        public string? CodigoTipoCota { get; set; }
        public string? NomeTipoCota { get; set; }
        public int? GrupoCota { get; set; }
        public string? CodigoGrupoCota { get; set; }
        public string? NomeGrupoCota { get; set; }
        public DateTime? DataAquisicao { get; set; }
        public int? ContratoTSEId { get; set; }
        public string? CodigoNumerico { get; set; }
        public string? IdIntercambiadora { get; set; } 
        public string? PadraoDeCor { get; set; } = "Default";
    }
}
