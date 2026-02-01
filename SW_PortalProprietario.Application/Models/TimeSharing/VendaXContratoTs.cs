namespace SW_PortalProprietario.Application.Models.TimeSharing
{
    public class VendaXContratoTs
    {
        public int? IdCliente { get; set; }
        public int? IdVendaXContrato { get; set; }
        public DateTime? Data { get; set; }
        public int? NumeroContrato { get; set; }
        public int? NumeroProjeto { get; set; }
        public int? IdContratoTs { get; set; }
        public DateTime? DataIntegraliza { get; set; }
        public DateTime? DataAniversario { get; set; }
        public int? IdontratoTs { get; set; }
        public int? Validade {  get; set; }
        public string? TipoValidade { get; set; }
        public int? IdTipoDcTaxa { get; set; }
        public string? FlgRevertido { get; set; }
        public string? FlgCancelado { get; set; }
    }
}
