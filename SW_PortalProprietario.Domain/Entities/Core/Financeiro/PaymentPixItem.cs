namespace SW_PortalProprietario.Domain.Entities.Core.Financeiro
{
    public class PaymentPixItem : EntityBaseCore, IEntityValidateCore
    {
        public virtual PaymentPix? PaymentPix { get; set; }
        public virtual decimal? Valor { get; set; }
        public virtual decimal? ValorNaTransacao { get; set; }
        public virtual DateTime? Vencimento { get; set; }
        public virtual string? ItemId { get; set; }
        public virtual string? DescricaoDoItem { get; set; }

        public virtual async Task SaveValidate()
        {
            List<string> mensagens = new();

            if (PaymentPix == null)
                mensagens.Add($"Deve ser informado o PaymentPix");

            if (Valor.GetValueOrDefault(0) <= 0)
                mensagens.Add($"O valor deve ser informado e maior que zero.");

            if (string.IsNullOrEmpty(ItemId))
                mensagens.Add($"O ItemId deve ser informado");

            if (string.IsNullOrEmpty(DescricaoDoItem))
                mensagens.Add($"A Descrição do item deve ser informada");


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
