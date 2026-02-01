namespace CMDomain.Models.Fornecedor
{
    public class TelefoneUpdateInputModel : ModelRequestBase
    {
        public int? IdTelefone { get; set; }
        public int? IdFornecedor { get; set; }
        public int? IdEmpresa { get; set; }
        public bool? Comercial { get; set; } = false;
        public bool? Partiticular { get; set; } = false;
        public bool? Fax { get; set; } = false;
        public bool? Celular { get; set; } = false;
        public bool? Recado { get; set; } = false;
        public string? Ddi { get; set; }
        public string? Ddd { get; set; }
        public string? Numero { get; set; }

    }
}
