namespace SW_PortalProprietario.Domain.Entities.Core.Geral
{
    public class ImagemGrupoImagemHomeTags : EntityBaseCore, IEntityValidateCore
    {
        public virtual Tags? Tags { get; set; }
        public virtual ImagemGrupoImagemHome? ImagemGrupoImagemHome { get; set; }

        public virtual async Task SaveValidate()
        {
            List<string> mensagens = new();

            if (Tags == null)
                mensagens.Add($"A Tag deve ser informada");

            if (ImagemGrupoImagemHome == null)
                mensagens.Add($"A ImagemGrupoImagemHome deve ser informada");

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

