namespace SW_PortalProprietario.Domain.Entities.Core.Geral
{
    public class GrupoImagemTags : EntityBaseCore, IEntityValidateCore
    {
        public virtual Tags? Tags { get; set; }
        public virtual GrupoImagem? GrupoImagem { get; set; }
        public virtual DateTime? DataHoraRemocao { get; set; }
        public virtual int? UsuarioRemocao { get; set; }

        public virtual async Task SaveValidate()
        {
            List<string> mensagens = new();


            if (Tags == null)
                mensagens.Add($"A Tag deve ser informada");

            if (GrupoImagem == null)
                mensagens.Add($"O Grupo imagem deve ser informado");

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
