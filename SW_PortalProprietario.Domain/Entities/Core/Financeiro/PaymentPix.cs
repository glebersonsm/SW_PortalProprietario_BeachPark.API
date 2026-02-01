using SW_PortalProprietario.Domain.Entities.Core.DadosPessoa;

namespace SW_PortalProprietario.Domain.Entities.Core.Financeiro
{
    public class PaymentPix : EntityBaseCore, IEntityValidateCore
    {
        public virtual string? CompanyId { get; set; }
        public virtual decimal? Valor { get; set; }
        public virtual string? Acquirer { get; set; }
        public virtual Pessoa? Pessoa { get; set; }
        public virtual string? Status { get; set; }
        public virtual string? PaymentId { get; set; }
        public virtual string? Pdf { get; set; }
        public virtual string? Payment_Id { get; set; }
        public virtual string? QrCode { get; set; }
        public virtual string? TransactionId { get; set; }
        public virtual string? Url { get; set; }
        public virtual string? Retorno { get; set; }
        public virtual string? DadosEnviados { get; set; }
        public virtual DateTime? ValidoAte { get; set; }
        public virtual int? AgrupamentoBaixaLegadoId { get; set; }
        public virtual int? EmpresaLegadoId { get; set; }


        public virtual async Task SaveValidate()
        {
            List<string> mensagens = new();

            if (!string.IsNullOrEmpty(Status) && new List<string>() { "cancelled", "captured", "pending" }.Any(b =>
            b.Equals(Status, StringComparison.InvariantCultureIgnoreCase)))
            {
                if (string.IsNullOrEmpty(QrCode))
                    mensagens.Add($"O QrCode deve ser informado");

                if (string.IsNullOrEmpty(Payment_Id))
                    mensagens.Add($"O Payment_Id deve ser informado");

                if (string.IsNullOrEmpty(Status))
                    mensagens.Add($"O Status deve ser informado");

                if (string.IsNullOrEmpty(Url))
                    mensagens.Add($"A Url deve ser informada");

                if (string.IsNullOrEmpty(TransactionId))
                    mensagens.Add($"A Transaction deve ser informada");

                if (string.IsNullOrEmpty(CompanyId))
                    mensagens.Add($"A CompanyId deve ser informada");


                if (string.IsNullOrEmpty(Acquirer))
                    mensagens.Add($"O Acquirer deve ser informado");

            }

            if (string.IsNullOrEmpty(Retorno))
                mensagens.Add($"O Response deve ser informado");


            if (Pessoa == null)
                mensagens.Add("A Pessoa deve ser informada.");


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
