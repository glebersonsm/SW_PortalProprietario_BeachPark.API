namespace SW_PortalProprietario.Application.Models.TransacoesFinanceiras.Boleto
{
    public class BoletoCedenteModel
    {
        public string? CPFCNPJ { get; set; }
        public string? Nome { get; set; }
        public BoletoEnderecoModel Endereco { get; set; } = new BoletoEnderecoModel();
        public BoletoContaBancariaModel ContaBancaria { get; set; } = new BoletoContaBancariaModel();
    }
}
