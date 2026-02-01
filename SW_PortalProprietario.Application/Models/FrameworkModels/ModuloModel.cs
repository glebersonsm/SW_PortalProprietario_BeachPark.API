using SW_PortalProprietario.Domain.Entities.Core.Framework;

namespace SW_PortalProprietario.Application.Models.FrameworkModels
{
    public class ModuloModel : ModelBase
    {
        public string? Codigo { get; set; }
        public string? Nome { get; set; }
        public int? GrupoModulo { get; set; }
        public string? GrupoModuloNome { get; set; }
        public int? AreaSistema { get; set; }
        public string? AreaSistemaNome { get; set; }
        public List<ModuloPermissaoModel>? Permissoes { get; set; }

        public static explicit operator ModuloModel(Modulo model)
        {
            return new ModuloModel
            {
                Id = model.Id,
                UsuarioCriacao = model.UsuarioCriacao,
                DataHoraCriacao = model.DataHoraCriacao,
                UsuarioAlteracao = model.UsuarioAlteracao,
                DataHoraAlteracao = model.DataHoraAlteracao,
                Codigo = model.Codigo,
                Nome = model.Nome,
                GrupoModulo = model.GrupoModulo?.Id,
                GrupoModuloNome = model.GrupoModulo?.Nome,
                AreaSistema = model.AreaSistema?.Id,
                AreaSistemaNome = model.AreaSistema?.Nome
            };
        }
    }
}
