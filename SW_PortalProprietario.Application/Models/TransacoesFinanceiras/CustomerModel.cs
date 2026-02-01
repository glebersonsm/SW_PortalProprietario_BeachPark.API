namespace SW_PortalProprietario.Application.Models.TransacoesFinanceiras
{
    public class CustomerModel
    {
        public int? rid { get; set; } //Identificador único do cliente
        public string? type { get; set; } //F = Fisica, J = Jurídica
        public string? name { get; set; }
        public string? document { get; set; }
        public string? document_type { get; set; } //CPF, CNPJ
        public string? email { get; set; }
        public List<PhoneModel> phones { get; set; } = new List<PhoneModel>();
        public AddressModel? address { get; set; }
        public string? created { get; set; }
        public bool? registered { get; set; } = true;
        public bool? foreigner { get; set; } = false;
        public string? birth { get; set; }
        public string? gender { get; set; } //M, F

    }
}
