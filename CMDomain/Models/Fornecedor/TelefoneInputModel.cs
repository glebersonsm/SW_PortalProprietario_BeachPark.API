namespace CMDomain.Models.Fornecedor
{
    public class TelefoneInputModel
    {
        public bool? Comercial { get; set; } = false;
        public bool? Particular { get; set; } = false;
        public bool? Fax { get; set; } = false;
        public bool? Celular { get; set; } = false;
        public bool? Recado { get; set; } = false;
        public string? Ddi { get; set; }
        public string? Ddd { get; set; }
        public string? Numero { get; set; }

    }
}
