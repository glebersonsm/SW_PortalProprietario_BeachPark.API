namespace SW_PortalProprietario.Domain.Entities.Core.DadosPessoa
{
    public class TipoEndereco : EntityBaseCore, IEntityValidateCore
    {
        public virtual string? Nome { get; set; }

        public virtual async Task SaveValidate()
        {
            List<string> mensagens = new();
            if (string.IsNullOrEmpty(Nome))
                mensagens.Add($"O Nome do Tipo de Endereço deve ser informado");

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
