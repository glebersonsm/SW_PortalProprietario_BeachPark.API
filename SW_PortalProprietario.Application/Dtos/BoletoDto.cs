namespace SW_PortalProprietario.Application.Dtos
{
    public class BoletoDto
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
