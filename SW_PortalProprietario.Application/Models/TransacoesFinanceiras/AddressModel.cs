namespace SW_PortalProprietario.Application.Models.TransacoesFinanceiras
{
    public class AddressModel
    {
        public string? street { get; set; } //"Logradouro Rua.."
        public string? number { get; set; }
        public string? neighborhood { get; set; } //Setor
        public string? zip_code { get; set; } //CEP
        public string? city { get; set; } //Nome da Cidade
        public string? state { get; set; } //Nome do Estado da Federação
        public string? country { get; set; } //BR para Brasil

    }
}
