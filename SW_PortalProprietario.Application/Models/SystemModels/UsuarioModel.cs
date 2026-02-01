using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Models.PessoaModels;
using SW_PortalProprietario.Domain.Entities.Core.Sistema;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Models.SystemModels
{
    public class UsuarioModel : ModelBase
    {
        public int? PessoaId { get; set; }
        public int? UsuarioId { get; set; }
        public string? NomePessoa { get; set; }
        public string? Login { get; set; }
        public EnumStatus? Status { get; set; }
        public EnumSimNao? Administrador { get; set; }
        public EnumSimNao? GestorFinanceiro { get; set; }
        public EnumSimNao? GestorReservasAgendamentos { get; set; }
        public PessoaCompletaModel? Pessoa { get; set; }
        public List<UsuarioModuloPermissaoModel>? UsuarioModuloPermissoes { get; set; }
        public List<EmpresaUsuarioModel>? UsuarioEmpresas { get; set; }
        public List<UsuarioGrupoUsuarioModel>? UsuarioGruposUsuarios { get; set; }
        public List<UsuarioTagsModel>? TagsRequeridas { get; set; }
        public string? ProviderChaveUsuario { get; set; }
        public string? PessoaProviderId { get; set; }
        public string? NomeProvider { get; set; }
        public EnumSimNao? IntegradoComTimeSharing { get; set; }
        public EnumSimNao? IntegradoComMultiPropriedade { get; set; }
        public string? LoginPms { get; set; }
        public string? LoginSistemaVenda { get; set; }

        public static explicit operator UsuarioModel(Usuario model)
        {
            return new UsuarioModel
            {
                Id = model.Id,
                UsuarioCriacao = model.UsuarioCriacao,
                DataHoraCriacao = model.DataHoraCriacao,
                Login = model.Login,
                NomePessoa = model.Pessoa?.Nome,
                Status = model.Status,
                Administrador = model.Administrador,
                LoginPms = model.LoginPms,
                LoginSistemaVenda = model.LoginSistemaVenda
            };
        }

    }
}
