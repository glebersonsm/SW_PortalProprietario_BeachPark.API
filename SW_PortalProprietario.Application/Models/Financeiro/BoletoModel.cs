namespace SW_PortalProprietario.Application.Models.Financeiro
{
    public class BoletoModel
    {
        public int Id { get; set; }
        public string? NomePessoa { get; set; }
        public string? CpfCnpjPessoa { get; set; }
        public decimal? Valor { get; set; }
        public DateTime? Vencimento { get; set; }
        public string? LinhaDigitavelBoleto { get; set; }
        public string? Path { get; set; }

    }
}
