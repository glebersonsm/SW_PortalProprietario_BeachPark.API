using SW_PortalProprietario.Application.Models.TimeSharing;

namespace SW_PortalProprietario.Application.Models.SystemModels
{
    /// <summary>
    /// OpÃ§Ãµes disponÃ­veis para configuraÃ§Ã£o de reservas VHF (PMS).
    /// Utilizado para popular dropdowns na tela de configuraÃ§Ã£o e integraÃ§Ã£o com sistemas legados.
    /// </summary>
    public class VhfConfigOpcoesModel
    {

        /// <summary>
        /// Tipo de negÃ³cio: Todos os negÃ³cios | Timesharing | Multipropriedade.
        /// ValidaÃ§Ã£o: valor deve estar na lista de opÃ§Ãµes.
        /// </summary>
        public List<VhfConfigOpcaoItem> TipoNegocio { get; set; } = new();

        /// <summary>
        /// Tipo de utilizaÃ§Ã£o: Uso prÃ³prio | Uso convidado.
        /// ValidaÃ§Ã£o: valor deve estar na lista de opÃ§Ãµes.
        /// </summary>
        public List<VhfConfigOpcaoItem> TipoUtilizacao { get; set; } = new();

        /// <summary>
        /// HotÃ©is/Unidades (CM): identificador da unidade.
        /// ValidaÃ§Ã£o: Id deve existir no cadastro de Empresas.
        /// </summary>
        public List<HotelModel> Hoteis { get; set; } = new();

        /// <summary>
        /// Tipo de HÃ³spede (CM): categoria padrÃ£o (ex: Lazer, Corporativo).
        /// ValidaÃ§Ã£o: cÃ³digo deve existir no sistema legado.
        /// </summary>
        public List<TipoHospedeModel> TiposHospede { get; set; } = new();

        /// <summary>
        /// Origem (CM): canal de venda padrÃ£o (ex: Direto, Motor de Reservas).
        /// ValidaÃ§Ã£o: cÃ³digo deve existir no sistema legado.
        /// </summary>
        public List<OrigemReservaModel> Origens { get; set; } = new();

        /// <summary>
        /// Tarifa Hotel (CM): cÃ³digo da tarifa base (ex: BAR, Acordo).
        /// ValidaÃ§Ã£o: cÃ³digo deve existir no cadastro de tarifas do PMS.
        /// </summary>
        public List<TarifaHotelModel> TarifasHotel { get; set; } = new();

        /// <summary>
        /// Segmento Reserva (CM): cÃ³digo do Segmento de reserva
        /// ValidaÃ§Ã£o: cÃ³digo deve existir no cadastro de Segmento do PMS.
        /// </summary>
        public List<SegmentoReservaModel> SegmentoReserva { get; set; } = new();

        /// <summary>
        /// CÃ³digo de PensÃ£o PadrÃ£o: regime de alimentaÃ§Ã£o (ex: CafÃ© da ManhÃ£, MAP).
        /// ValidaÃ§Ã£o: cÃ³digo deve existir no cadastro de pensÃµes do PMS.
        /// </summary>
        public List<VhfConfigOpcaoItem> CodigosPensao { get; set; } = new();
    }

    /// <summary>
    /// Item de opÃ§Ã£o para dropdown (valor + label).
    /// </summary>
    public class VhfConfigOpcaoItem
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }
}
