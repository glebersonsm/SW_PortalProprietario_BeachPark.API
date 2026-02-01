namespace SW_PortalProprietario.Application.Models.TimeSharing
{
    public class LancamentosFrontModel
    {
        public int? IdLancamento { get; set; }
        public int? IdHotel { get; set; }
        public int? NomeHotel { get; set; }
        public int? IdReservasFront { get; set; }
        public int? IdConta { get; set; }
        public string? DesignacaoConta { get; set; }
        public int? IdHospede { get; set; }
        public string? NomeHospede { get; set; }
        public string? SobreNomeHospede { get; set; }
        public int? IdTipoLancamento { get; set; }
        public string? NomeTipoLancamento { get; set; }
        public DateTime? DataLancamento { get; set; }
        public int? NumParcelas { get; set; }
        public string? Documento { get; set; }
        public decimal? VlrLancamento { get; set; }
        public DateTime? TrgDtInclusao { get; set; }
        public string? TrgUserInclusao { get; set; }
    }
}
