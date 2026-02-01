using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Models.SystemModels
{
    public class UsuarioSearchModel
    {
        public int? Id { get; set; }
        public string? Login { get; set; }
        public string? Email { get; set; }
        public string? LoginNomeEmail { get; set; }
        public string? CpfCnpj { get; set; }
        public string? NomePessoa { get; set; }
        public EnumStatus? Status { get; set; }
        public bool? CarregarEmpresas { get; set; }
        public bool? CarregarPermissoes { get; set; }
        public bool? CarregarGruposDeUsuarios { get; set; }
        public bool? CarregarDadosPessoa { get; set; }

    }
}
