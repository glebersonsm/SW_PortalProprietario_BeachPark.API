namespace SW_PortalProprietario.Domain.Entities.Core.Geral
{
    public class Tags : EntityBaseCore, IEntityValidateCore
    {
        public virtual Tags? Parent { get; set; }
        public virtual string? Nome { get; set; }
        public virtual string? Path { get; set; }

        public virtual async Task SaveValidate()
        {
            List<string> mensagens = new();


            if (string.IsNullOrEmpty(Nome))
                mensagens.Add($"O Nome deve ser informado");

            if (string.IsNullOrEmpty(Path) && Id > 0)
                mensagens.Add($"A Path deve ser informada");

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
