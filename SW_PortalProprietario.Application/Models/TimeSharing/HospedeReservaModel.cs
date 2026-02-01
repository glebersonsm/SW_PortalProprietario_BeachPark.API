namespace SW_PortalProprietario.Application.Models.TimeSharing
{
    public class HospedeReservaModel
    {
        public int? IdHotel { get; set; }
        public int? NomeHotel { get; set; }
        public int? IdReservasFront { get; set; }
        public int? IdHospede { get; set; }
        public string? NomeHospede { get; set; }
        public string? SobreNomeHospede { get; set; }
        public string? Principal { get; set; }
        public int? IdTipoHospede { get; set; }
        public string? TipoHospedeNome { get; set; }
        public DateTime? DataChegadaPrevista { get; set; }
        public DateTime? DataChegadaReal { get; set; }
        public DateTime? DataPartidaPrevista { get; set; }
        public DateTime? DataPartidaReal { get; set; }
        public string? MenorIdade { get; set; }
        public DateTime? TrgDtInclusao { get; set; }
        public string? TrgUserInclusao { get; set; }

        public List<LancamentosFrontModel>? Lancamentos { get; set; }
    }
}
