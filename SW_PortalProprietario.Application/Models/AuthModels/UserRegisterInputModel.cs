using SW_PortalProprietario.Application.Auxiliar;
using SW_PortalProprietario.Domain.Enumns;
using System.Text.Json.Serialization;

namespace SW_PortalProprietario.Application.Models.AuthModels
{
    public class UserRegisterInputModel
    {
        public string? Login { get; set; }
        public string? StatusContrato { get; set; }
        public string? FullName { get; set; }
        public string? PessoaId { get; set; }
        public string? CpfCnpj { get; set; }
        public string? TipoDocumentoClienteNome { get; set; }
        public DateTime? DataNascimento { get; set; }
        public string? Email { get; set; }
        [JsonConverter(typeof(EncryptConverter))]
        public string? Password { get; set; }
        [JsonConverter(typeof(EncryptConverter))]
        public string? PasswordConfirmation { get; set; }
        /// <summary>
        /// Incluido apenas para forçar pegar todas as permissões (Essa função só pode ser utilizada em Debug)
        /// </summary>
        public EnumSimNao? Administrator { get; set; }
        public string? Codigo { get; set; }
        public string? Estrangeiro { get; set; }
        public int? EmpresaFinanceiroId { get; set; }
        public int? PessoaFinanceiroId { get; set; }
        public string? PessoaFinanceiroNome { get; set; }
        public string? Sexo { get; set; }

    }
}
