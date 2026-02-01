namespace EsolutionPortalDomain.ReservasApiModels.Condominio
{
    public class CotaEsolutionPortalModel
    {
        public int CotaId { get; set; }
        public string? Produto { get; set; }
        public string? Numero { get; set; }
        public string? TagCota { get; set; } //<Importado=S>|<PORTAL=>
        public int? GrupoCotaTipoCota { get; set; }
        public int? Imovel { get; set; }
        public string? ImovelNumero { get; set; }
        public string? NumeroImovel { get; set; }
        public string? TagImovel { get; set; }
        public int? ImovelBloco { get; set; }
        public string? CodigoBloco { get; set; }
        public string? NomeBloco { get; set; }
        public int? Proprietario { get; set; }
        public int? Procurador { get; set; }
        public string? StatusCota { get; set; } = "D";
        public string? CotaBloqueada { get; set; }
        public int? FrAtendimentoVenda { get; set; }
        public int? CategoriaCota { get; set; }
        public string? CodigoCategoriaCota { get; set; }
        public string? NomeCategoriaCota { get; set; }
        public string? GrupoCotaTipoCotaCodigo { get; set; }
        public string? GrupoCotaTipoCotaNome { get; set; }
        public int? TipoCota { get; set; }
        public string? TipoCotaCodigo { get; set; }
        public string? TipoCotaNome { get; set; }
        public string? GrupoCotaCodigo { get; set; }
        public string? GrupoCotaNome { get; set; }
        public int? GrupoCota { get; set; }
        public string? CodigoGrupoCota { get; set; }
        public string? NomeGrupoCota { get; set; }
        public DateTime? DataAquisicao { get; set; }
        public int? ContratoTSEId { get; set; }
        public string? CodigoNumerico { get; set; }
        public string? AndarCodigo { get; set; }
        public string? AndarNome { get; set; }
    }
}
