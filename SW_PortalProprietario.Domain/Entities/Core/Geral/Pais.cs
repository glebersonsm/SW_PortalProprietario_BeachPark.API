namespace SW_PortalProprietario.Domain.Entities.Core.Geral
{
    public class Pais : EntityBaseCore, IEntityValidateCore
    {
        public virtual string? Nome { get; set; }
        public virtual string? CodigoIbge { get; set; }
        public virtual string? Ddi { get; set; }
        public virtual string? MascaraTelefoneCelular { get; set; }
        public virtual string? MascaraTelefoneFixo { get; set; }

        public virtual async Task SaveValidate()
        {
            List<string> mensagens = new();

            if (string.IsNullOrEmpty(Nome))
                mensagens.Add($"O Nome do País deve ser informado");

            if (string.IsNullOrEmpty(CodigoIbge))
                mensagens.Add($"O Código do País no IBGE deve ser informado");
            if (string.IsNullOrEmpty(MascaraTelefoneFixo))
                mensagens.Add($"A Máscara para Telefone Fixo deve ser informado");
            if (string.IsNullOrEmpty(MascaraTelefoneCelular))
                mensagens.Add($"A Máscara para Telefone Celular deve ser informado");

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
