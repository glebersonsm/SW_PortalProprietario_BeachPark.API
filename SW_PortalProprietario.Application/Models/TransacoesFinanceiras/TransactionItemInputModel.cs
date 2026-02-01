namespace SW_PortalProprietario.Application.Models.TransacoesFinanceiras
{
    public class TransactionItemInputModel
    {
        public string? item_id { get; set; } //Id do item que está sendo transacionado
        public decimal? value { get; set; }
        public string? name { get; set; }
        public decimal? amount { get; set; }
        public string? date { get; set; }
        public string? end_date { get; set; }
        public int? travelers_amount { get; set; }//Quantidade de pessoas 
        public string? category { get; set; } //Tickets, ver oque enviar para pagamento de contas
        public List<CustomerModel>? travelers { get; set; }
    }
}
