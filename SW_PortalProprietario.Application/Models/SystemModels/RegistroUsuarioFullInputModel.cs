using SW_PortalProprietario.Application.Models.PessoaModels;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Models.SystemModels
{
    public class RegistroUsuarioFullInputModel : CreateUpdateModelBase
    {
        public PessoaFisicaInputModel? Pessoa { get; set; }
        public string? Login { get; set; }
        public EnumStatus? Status { get; set; } = EnumStatus.Ativo;
        public List<int>? UsuarioEmpresas { get; set; }
        public bool RemoverEmpresasNaoEnviadas { get; set; } = false;
        public List<int>? UsuarioGruposUsuarios { get; set; }
        public bool RemoverGrupoUsuariosNaoEnviados { get; set; } = false;
        public EnumSimNao? Administrador { get; set; }
        public EnumSimNao? GestorFinanceiro { get; set; }
        public EnumSimNao? GestorReservasAgendamentos { get; set; }
        public EnumSimNao? UsuarioAdministrativo { get; set; }
        public bool RemoverTagsNaoEnviadas { get; set; } = false;
        public List<int>? TagsRequeridas { get; set; }
        public string? LoginPms { get; set; }
        public string? LoginSistemaVenda { get; set; }
        public string? AvatarBase64 { get; set; }
        public List<string>? MenuPermissions { get; set; }


    }
}
