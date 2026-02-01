using SW_PortalProprietario.Domain.Entities.Core.Sistema;

namespace SW_PortalProprietario.Domain.Entities.Core.Geral
{
    public class UsuarioTags : EntityBaseCore, IEntityValidateCore
    {
        public virtual Tags? Tags { get; set; }
        public virtual Usuario? Usuario { get; set; }

        public virtual async Task SaveValidate()
        {
            List<string> mensagens = new();


            if (Tags == null)
                mensagens.Add($"A Tag deve ser informada");

            if (Usuario == null)
                mensagens.Add($"O Usuario deve ser informado");

            if (mensagens.Any())
                await Task.FromException(new ArgumentException(string.Join($"{Environment.NewLine}", mensagens.Select(a => a).ToList())));
            else await Task.CompletedTask;
        }

        public virtual object Clone()
        {
            return MemberwiseClone();
        }
    }
}
