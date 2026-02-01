namespace SW_PortalProprietario.Application.Models.TransacoesFinanceiras
{
    public class DoTransactionPixInputModel
    {
        public int? PessoaId { get; set; }
        public decimal? ValorTotal { get; set; }
        public List<int> ItensToPay { get; set; } = new List<int>();

    }
}
