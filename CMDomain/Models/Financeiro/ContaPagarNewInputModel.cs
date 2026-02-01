namespace CMDomain.Models.Financeiro
{
    public class ContaPagarNewInputModel : ModelRequestBase
    {
        public int? IdEmpresa { get; set; }
        public int? IdFornecedor { get; set; }
        public int? CodTipoDocumento { get; set; }
        public string? NumeroDocumento { get; set; }
        public string? ComplDocumento { get; set; }
        public DateTime? DataEmissao { get; set; }
        public decimal? ValorDocumento { get; set; }
        public string? Cfop { get; set; }
        public int? IdModeloDocumento { get; set; }
        public string? SerieDocumento { get; set; }
        public string? SubSerieDocumento { get; set; }
        public string? CodTipoLigacao { get; set; }
        public string? CodClasseConsumo { get; set; }
        public string? CodGrupoTensao { get; set; }
        public string? NumeroGuia { get; set; }
        public string? Placa { get; set; }
        public string? ObsLancamento { get; set; }
        public string? HistoricoLanc { get; set; }
        public List<ContaPagarAlteradorValorInputModel> AlteradoresDocumento { get; set; } = new List<ContaPagarAlteradorValorInputModel>();
        public List<ContaPagarParcelaInputModel> Parcelas { get; set; } = new List<ContaPagarParcelaInputModel>();
        public List<ContaPagarRateioInputModel> Rateio { get; set; } = new List<ContaPagarRateioInputModel>();

    }
}
