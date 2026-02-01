
namespace CMDomain.Models.Fornecedor
{
    public class TelefoneViewModel
    {
        public int? IdTelefone { get; set; }
        public int? IdEndereco { get; set; }
        public int? IdFornecedor { get; set; }
        public bool? Comercial { get; set; } = false;
        public bool? Partiticular { get; set; } = false;
        public bool? Fax { get; set; } = false;
        public bool? Celular { get; set; } = false;
        public bool? Recado { get; set; } = false;
        public string? Ddi { get; set; }
        public string? Ddd { get; set; }
        public string? Numero { get; set; }
        public DateTime? DataHoraCriacao { get; set; }
        public string? UsuarioCriacao { get; set; }

    }
}
