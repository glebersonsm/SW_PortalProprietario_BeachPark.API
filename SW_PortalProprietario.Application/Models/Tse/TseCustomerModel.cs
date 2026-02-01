namespace SW_PortalProprietario.Application.Models.Tse
{
    public class TseCustomerModel
    {
        public string? IdPessoa { get; set; }
        public DateTime? DataCadastro { get; set; }
        public string? TipoPessoa { get; set; }
        public string? Email { get; set; }
        public string? RazaoSocial { get; set; }
        public string? NomeFantasia { get; set; }
        public DateTime? DataNascimento { get; set; }
        public string? CpfCnpj { get; set; }
        public string? Sexo { get; set; }
        public string? Nome { get; set; }
        public string? Cliente { get; set; }
        public int? IdContrato { get; set; }
        public string? NumeroContrato { get; set; }
        public string Administrador { get; set; } = "N";

    }
}
