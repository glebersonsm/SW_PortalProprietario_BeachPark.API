namespace SW_PortalProprietario.Application.Models.GeralModels
{
    public class RegraPaxFreeInputModel : CreateUpdateModelBase
    {
        public string? Nome { get; set; }
        public DateTime? DataInicioVigencia { get; set; }
        public DateTime? DataFimVigencia { get; set; }
        public bool? RemoverConfiguracoesNaoEnviadas { get; set; } = false;
        public List<RegraPaxFreeConfiguracaoInputModel>? Configuracoes { get; set; }
        public List<RegraPaxFreeHotelInputModel>? Hoteis { get; set; }
        public bool? RemoverHoteisNaoEnviados { get; set; } = false;
    }
}

