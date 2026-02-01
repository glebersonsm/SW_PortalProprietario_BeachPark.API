namespace SW_PortalProprietario.Domain.Entities.Core.Sistema
{
    public class UsuarioGrupoUsuario : EntityBaseCore, IEntityValidateCore
    {
        public virtual Usuario? Usuario { get; set; }
        public virtual GrupoUsuario? GrupoUsuario { get; set; }

        public virtual async Task SaveValidate()
        {
            List<string> mensagens = new();

            if (Usuario == null)
                mensagens.Add("O Usuário deve ser informado no Usuário Grupo Usuário");

            if (GrupoUsuario == null)
                mensagens.Add($"O Grupo Usuario deve ser informado");


            if (mensagens.Any())
                await Task.FromException(new ArgumentException(string.Join($"{Environment.NewLine}", mensagens.Select(a => a).ToList())));
            else await Task.CompletedTask;
        }
    }
}
