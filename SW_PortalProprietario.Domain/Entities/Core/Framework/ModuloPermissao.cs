namespace SW_PortalProprietario.Domain.Entities.Core.Framework
{
    public class ModuloPermissao : EntityBaseCore, IEntityValidateCore
    {
        public virtual Modulo? Modulo { get; set; }
        public virtual Permissao? Permissao { get; set; }

        public virtual async Task SaveValidate()
        {
            List<string> mensagens = new();


            if (Modulo == null)
                mensagens.Add($"O Módulo do Módulo Permissão deve ser informado");

            if (Permissao == null)
                mensagens.Add($"A Permissão do Módulo Permissão deve ser informada");

            if (mensagens.Any())
                await Task.FromException(new ArgumentException(string.Join($"{Environment.NewLine}", mensagens.Select(a => a).ToList())));
            else await Task.CompletedTask;
        }
    }
}
