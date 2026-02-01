namespace SW_PortalProprietario.Domain.Entities.Core.Geral
{
    public class ImagemGrupoImagemHome : EntityBaseCore, IEntityValidateCore
    {
        public virtual GrupoImagemHome? GrupoImagemHome { get; set; }
        public virtual string? Nome { get; set; }
        public virtual byte[]? Imagem { get; set; }
        public virtual string? NomeBotao { get; set; }
        public virtual string? LinkBotao { get; set; }
        public virtual int? Ordem { get; set; }
        public virtual DateTime? DataInicioVigencia { get; set; }
        public virtual DateTime? DataFimVigencia { get; set; }

        public virtual async Task SaveValidate()
        {
            List<string> mensagens = new();

            if (GrupoImagemHome == null)
                mensagens.Add("O GrupoImagemHome deve ser informado");

            if (string.IsNullOrEmpty(Nome))
                mensagens.Add($"O Nome deve ser informado");

            if (Id == 0 && (Imagem == null || Imagem.Length == 0))
                mensagens.Add($"A Imagem deve ser informada");

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

