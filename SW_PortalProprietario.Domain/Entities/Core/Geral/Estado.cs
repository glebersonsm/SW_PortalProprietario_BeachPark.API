namespace SW_PortalProprietario.Domain.Entities.Core.Geral
{
    public class Estado : EntityBaseCore, IEntityValidateCore
    {
        public virtual Pais? Pais { get; set; }
        public virtual string? Nome { get; set; }
        public virtual string? Sigla { get; set; }
        public virtual string? CodigoIbge { get; set; }

        public virtual async Task SaveValidate()
        {
            List<string> mensagens = new();

            if (Pais == null)
                mensagens.Add("O País deve ser informado no Estado");

            if (string.IsNullOrEmpty(Nome))
                mensagens.Add($"O Nome do Estado deve ser informado");

            if (string.IsNullOrEmpty(Sigla))
                mensagens.Add($"A Sigla do Estado deve ser informada");

            if (string.IsNullOrEmpty(CodigoIbge))
                mensagens.Add($"O Código do Estado no IBGE deve ser informado");

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
