namespace SW_PortalProprietario.Application.Models.SystemModels
{
    /// <summary>
    /// Opções disponíveis para configuração de reservas VHF (PMS).
    /// Utilizado para popular dropdowns na tela de configuração e integração com sistemas legados.
    /// </summary>
    public class VhfConfigOpcoesModel
    {
        /// <summary>
        /// Tipo de utilização: Uso próprio | Uso convidado.
        /// Validação: valor deve estar na lista de opções.
        /// </summary>
        public List<VhfConfigOpcaoItem> TipoUtilizacao { get; set; } = new();

        /// <summary>
        /// Hotéis/Unidades (CM): identificador da unidade.
        /// Validação: Id deve existir no cadastro de Empresas.
        /// </summary>
        public List<VhfConfigOpcaoItem> Hoteis { get; set; } = new();

        /// <summary>
        /// Tipo de Hóspede (CM): categoria padrão (ex: Lazer, Corporativo).
        /// Validação: código deve existir no sistema legado.
        /// </summary>
        public List<VhfConfigOpcaoItem> TiposHospede { get; set; } = new();

        /// <summary>
        /// Origem (CM): canal de venda padrão (ex: Direto, Motor de Reservas).
        /// Validação: código deve existir no sistema legado.
        /// </summary>
        public List<VhfConfigOpcaoItem> Origens { get; set; } = new();

        /// <summary>
        /// Tarifa Hotel (CM): código da tarifa base (ex: BAR, Acordo).
        /// Validação: código deve existir no cadastro de tarifas do PMS.
        /// </summary>
        public List<VhfConfigOpcaoItem> TarifasHotel { get; set; } = new();

        /// <summary>
        /// Código de Pensão Padrão: regime de alimentação (ex: Café da Manhã, MAP).
        /// Validação: código deve existir no cadastro de pensões do PMS.
        /// </summary>
        public List<VhfConfigOpcaoItem> CodigosPensao { get; set; } = new();
    }

    /// <summary>
    /// Item de opção para dropdown (valor + label).
    /// </summary>
    public class VhfConfigOpcaoItem
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }
}
