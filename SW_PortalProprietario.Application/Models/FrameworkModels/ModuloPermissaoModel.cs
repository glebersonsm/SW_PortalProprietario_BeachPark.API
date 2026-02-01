using SW_PortalProprietario.Domain.Entities.Core.Framework;

namespace SW_PortalProprietario.Application.Models.FrameworkModels
{
    public class ModuloPermissaoModel : ModelBase
    {
        public string? Nome { get; set; }
        public string? NomeInterno { get; set; }
        public string? TipoPermissao { get; set; }

        public static explicit operator ModuloPermissaoModel(ModuloPermissao model)
        {
            return new ModuloPermissaoModel
            {
                Id = model.Id,
                UsuarioCriacao = model.UsuarioCriacao,
                DataHoraCriacao = model.DataHoraCriacao,
                UsuarioAlteracao = model.UsuarioAlteracao,
                DataHoraAlteracao = model.DataHoraAlteracao,
                NomeInterno = model.Permissao?.NomeInterno,
                TipoPermissao = model.Permissao?.TipoPermissao
            };
        }
    }
}
