namespace SW_PortalProprietario.Domain.Entities.Core.Financeiro
{
    public class PaymentCardTokenized : EntityBaseCore, IEntityValidateCore
    {
        public virtual CardTokenized? CardTokenized { get; set; }
        public virtual decimal? Valor { get; set; }
        public virtual string? PaymentId { get; set; }
        public virtual string? Status { get; set; }
        public virtual string? Nsu { get; set; }
        public virtual string? CodigoAutorizacao { get; set; }
        public virtual string? Adquirente { get; set; }
        public virtual string? AdquirentePaymentId { get; set; }
        public virtual string? TransactionId { get; set; }
        public virtual string? Url { get; set; }
        public virtual string? CompanyId { get; set; }
        public virtual string? DadosEnviados { get; set; }
        public virtual string? Retorno { get; set; }
        public virtual int? ParcelasSincronizadas { get; set; }
        public virtual string? ResultadoSincronizacaoParcelas { get; set; }
        public virtual int? EmpresaLegadoId { get; set; }
        
        public virtual async Task SaveValidate()
        {
            List<string> mensagens = new();

            if (CardTokenized == null)
                mensagens.Add($"Deve ser informado o CardTokenized");

            if (Valor.GetValueOrDefault(0) <= 0)
                mensagens.Add($"O valor deve ser informado e maior que zero.");

            if (string.IsNullOrEmpty(PaymentId))
                mensagens.Add($"O PaymentId deve ser informado");

            if (string.IsNullOrEmpty(Status))
                mensagens.Add("O Status deve ser informado");

            if (string.IsNullOrEmpty(CodigoAutorizacao))
                mensagens.Add("O Código de autorização deve ser informado");

            if (string.IsNullOrEmpty(Adquirente))
                mensagens.Add("O Adquirente deve ser informado");

            if (string.IsNullOrEmpty(AdquirentePaymentId))
                mensagens.Add("O AdquirentePaymentId deve ser informado");

            if (string.IsNullOrEmpty(TransactionId))
                mensagens.Add("A TransactionId deve ser informada");

            if (string.IsNullOrEmpty(Url))
                mensagens.Add("A Url deve ser informada");

            if (string.IsNullOrEmpty(CompanyId))
                mensagens.Add("A CompanyId deve ser informada");


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
