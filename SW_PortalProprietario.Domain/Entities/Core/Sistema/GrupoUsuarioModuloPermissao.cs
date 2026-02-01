using SW_PortalProprietario.Domain.Entities.Core.Framework;

namespace SW_PortalProprietario.Domain.Entities.Core.Sistema
{
    public class GrupoUsuarioModuloPermissao : EntityBaseCore, IEntityValidateCore
    {
        public virtual GrupoUsuario? GrupoUsuario { get; set; }
        public virtual ModuloPermissao? ModuloPermissao { get; set; }

        public virtual async Task SaveValidate()
        {
            List<string> mensagens = new();

            if (GrupoUsuario == null)
                mensagens.Add("O Grupo Usuário deve ser informado");

            if (ModuloPermissao == null)
                mensagens.Add($"O Módulo Permissão deve ser informado");


            if (mensagens.Any())
                await Task.FromException(new ArgumentException(string.Join($"{Environment.NewLine}", mensagens.Select(a => a).ToList())));
            else await Task.CompletedTask;
        }
    }
}
