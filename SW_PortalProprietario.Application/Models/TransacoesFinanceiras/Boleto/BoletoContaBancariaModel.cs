namespace SW_PortalProprietario.Application.Models.TransacoesFinanceiras.Boleto
{
    public class BoletoContaBancariaModel
    {
        public string? Agencia { get; set; }
        public string? DigitoAgencia { get; set; }
        public string? Conta { get; set; }
        public string? DigitoConta { get; set; }
        public string? CarteiraPadrao { get; set; }
        public string? VariacaoCarteiraPadrao { get; set; }
        public string? OperacaoConta { get; set; }

    }
}
