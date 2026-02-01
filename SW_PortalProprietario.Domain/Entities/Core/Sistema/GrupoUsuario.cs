using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Domain.Entities.Core.Sistema
{
    public class GrupoUsuario : EntityBaseCore, IEntityValidateCore
    {
        public virtual string? Nome { get; set; }
        public virtual EnumStatus? Status { get; set; } = EnumStatus.Ativo;

        public virtual async Task SaveValidate()
        {
            List<string> mensagens = new();

            if (string.IsNullOrEmpty(Nome))
                mensagens.Add("O Nome do Grupo de Usuário deve ser informado");

            if (!Status.HasValue)
                mensagens.Add($"O Status do Grupo de Usuário deve ser informado");


            if (mensagens.Any())
                await Task.FromException(new ArgumentException(string.Join($"{Environment.NewLine}", mensagens.Select(a => a).ToList())));
            else await Task.CompletedTask;
        }
    }
}
