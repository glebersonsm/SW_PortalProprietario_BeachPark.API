namespace SW_PortalProprietario.Application.Models.TransacoesFinanceiras
{
    public class TransactionSimplifiedResultModel
    {
        public string? paymentId { get; set; }
        public string? PessoaId { get; set; }
        public string? PessoaNome { get; set; }
        public bool? Pix { get; set; }
        public bool? Cartao { get; set; }
        public decimal? ValorTransacao { get; set; }
        public string? Status { get; set; } //Cancelada, Autorizada, Pendente, Negada
        public DateTime? DataTransacao { get; set; }
        public string? Nsu { get; set; }
        public string? Autorizacao { get; set; }
        public string? Adquirente { get; set; }
        public string? TransactionId { get; set; }
        public string? QrCode { get; set; }
        public string? Url { get; set; }
        public int? InternalId { get; set; }
        public string? Chave { get; set; }
        public int? HashCode { get; set; }
        public string? TipoOperacao { get; set; }
        public string? DadosEnviados { get; set; }
        public string? Retorno { get; set; }
        public List<PaymentItemModel>? ContasVinculadas { get; set; } = new List<PaymentItemModel>();

    }
}
