namespace SW_PortalProprietario.Application.Models.TransacoesFinanceiras.Boleto
{
    public class BoletoSacadoModel
    {
        public string? CPFCNPJ { get; set; }
        public string? Nome { get; set; }
        public BoletoEnderecoModel Endereco { get; set; } = new BoletoEnderecoModel();
    }
}
