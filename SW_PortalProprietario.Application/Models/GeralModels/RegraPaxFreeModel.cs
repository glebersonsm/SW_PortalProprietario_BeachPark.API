namespace SW_PortalProprietario.Application.Models.GeralModels
{
    public class RegraPaxFreeModel : ModelBase
    {
        public string? Nome { get; set; }
        public DateTime? DataInicioVigencia { get; set; }
        public string? DataInicioVigenciaStr => DataInicioVigencia?.ToString("yyyy-MM-dd");
        public DateTime? DataFimVigencia { get; set; }
        public string? DataFimVigenciaStr => DataFimVigencia?.ToString("yyyy-MM-dd");
        public List<RegraPaxFreeConfiguracaoModel>? Configuracoes { get; set; }
        public List<RegraPaxFreeHotelModel>? Hoteis { get; set; }
    }
}

