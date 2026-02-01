using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Models.TransacoesFinanceiras
{
    public class PessoaParaTransacaoBrokerModel
    {
        public int? PessoaId { get; set; }
        public DateTime? DataNascimento { get; set; }
        public string? Nome { get; set; }
        public string? Email { get; set; }
        public string? Sexo { get; set; }
        public EnumTipoPessoa TipoPessoa { get; set; }
        public string? Cpf { get; set; }
        public string? Cnpj { get; set; }
        public string? CidadeNome { get; set; }
        public string? EstadoSigla { get; set; }
        public string? EstadoNome { get; set; }
        public string? Numero { get; set; }
        public string? Logradouro { get; set; }
        public string? Bairro { get; set; }
        public string? Complemento { get; set; }
        public string? Cep { get; set; }
        public string? TipoTelefone { get; set; }
        public string? NumeroTelefone { get; set; }
        public string? SiglaPais { get; set; }

    }
}

