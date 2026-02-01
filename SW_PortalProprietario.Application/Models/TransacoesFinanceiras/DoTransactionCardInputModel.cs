namespace SW_PortalProprietario.Application.Models.TransacoesFinanceiras
{
    public class DoTransactionCardInputModel
    {
        public int? PessoaId { get; set; }
        public int? CardTokenizedId { get; set; }
        public decimal? ValorTotal { get; set; }
        public List<int> ItensToPay { get; set; } = new List<int>();

    }
}
