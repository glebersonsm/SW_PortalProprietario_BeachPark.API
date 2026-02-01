using SW_PortalProprietario.Application.Models.AuthModels;

namespace SW_PortalProprietario.Application.Interfaces
{
    public interface IAccessValidateResultModel
    {
        string? ClienteId { get; set; }
        List<string> Erros { get; set; }
        List<string> InfomacoesAdicionais { get; set; }
        string? PessoaId { get; set; }
        string? PessoaNome { get; set; }
        int? UsuarioSistema { get; set; }
        string? ProviderName { get; set; }
        LoginResult? LoginResult { get; set; }
    }
}
