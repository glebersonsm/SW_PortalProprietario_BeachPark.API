using SW_PortalProprietario.Domain.Entities.Core.Framework;

namespace SW_PortalProprietario.Domain.Entities.Core.Sistema
{
    public class EmpresaUsuario : EntityBaseCore, IEntityValidateCore
    {
        public virtual Usuario? Usuario { get; set; }
        public virtual Empresa? Empresa { get; set; }

        public virtual async Task SaveValidate()
        {
            List<string> mensagens = new();

            if (Usuario == null)
                mensagens.Add("O Usuário deve ser informado");

            if (Empresa == null)
                mensagens.Add($"A Empresa deve ser informada");

            if (mensagens.Any())
                await Task.FromException(new ArgumentException(string.Join($"{Environment.NewLine}", mensagens.Select(a => a).ToList())));
            else await Task.CompletedTask;
        }
    }
}
