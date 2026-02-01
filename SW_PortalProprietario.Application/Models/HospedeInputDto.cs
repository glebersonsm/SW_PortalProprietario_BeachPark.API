namespace SW_PortalProprietario.Application.Models
{
    public class HospedeInputDto
    {
        public Int64? Id { get; set; }
        public Int64? IdHospede { get; set; }
        public string? TipoHospede { get; set; }
        public string? IdTipoHospede { get; set; }
        public Int64? ClienteId { get; set; }
        public string? Principal { get; set; }
        public string? Nome { get; set; }
        public string? Cpf { get; set; }
        public string? DataNascimento { get; set; }
        public string? Documento { get; set; }
        public string? TipoDocumento { get; set; }
        public int? TipoDocumentoId { get; set; }
        public string? Email { get; set; }
        public string? Sexo { get; set; }
        public string? DDI { get; set; }
        public string? DDD { get; set; }
        public string? Telefone { get; set; }
        public string? CodigoIbge { get; set; }
        public string? Logradouro { get; set; }
        public string? Numero { get; set; }
        public string? Bairro { get; set; }
        public string? CEP { get; set; }
        public int? CidadeId { get; set; }
        public string? CidadeNome { get; set; }
        public string? SiglaEstado { get; set; }
        public int? Estrangeiro { get; set; } //0 = não, 1 = Sim
        public string? CheckIn { get; set; }
        public string? CheckOut { get; set; }
        public string? CidadeUf { get; set; }
        

    }
}
