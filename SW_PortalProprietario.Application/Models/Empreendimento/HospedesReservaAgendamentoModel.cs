namespace SW_PortalProprietario.Application.Models.Empreendimento
{
    public class HospedesReservaAgendamentoModel
    {
        public int? Id { get; set; }
        public int? ClienteId { get; set; }
        public string? Principal { get; set; }

        public string? Nome { get; set; }
        public string? CPF { get; set; }
        public DateTime? DataNascimento { get; set; }
        public string? Email { get; set; }
        public string? Sexo { get; set; }

        public string? Logradouro { get; set; }
        public string? Numero { get; set; }
        public string? Bairro { get; set; }
        public string? Complemento { get; set; }
        public string? CidadeNome { get; set; }
        public string? UF { get; set; }
        public string? PaisNome { get; set; }
        public int? CidadeId { get; set; }
        public string? CidadeFormatada { get; set; }
    }
}
