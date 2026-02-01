using SW_PortalProprietario.Domain.Entities.Core.DadosPessoa;
using SW_PortalProprietario.Domain.Enumns;
using SW_PortalProprietario.Domain.Functions;

namespace SW_PortalProprietario.Domain.Entities.Core.Financeiro
{
    public class CardTokenized : EntityBaseCore, IEntityValidateCore
    {
        public virtual string? CardHolder { get; set; }
        public virtual string? Brand { get; set; }
        public virtual string? CardNumber { get; set; }
        public virtual string? Cvv { get; set; }
        public virtual string? DueDate { get; set; }
        public virtual string? Token { get; set; }
        public virtual string? Token2 { get; set; }
        public virtual string? Status { get; set; }
        public virtual string? CompanyId { get; set; }
        public virtual string? CompanyToken { get; set; }
        public virtual string? Acquirer { get; set; }
        public virtual string? ClienteId { get; set; }
        public virtual Pessoa? Pessoa { get; set; }
        public virtual string? Hash { get; set; }
        public virtual EnumSimNao? Visivel { get; set; }
        public virtual int? EmpresaLegadoId { get; set; }

        public virtual async Task SaveValidate()
        {
            List<string> mensagens = new();


            if (string.IsNullOrEmpty(CardHolder))
                mensagens.Add($"O CardHolder deve ser informado");

            if (string.IsNullOrEmpty(Brand))
                mensagens.Add($"A Brand deve ser informada");

            if (string.IsNullOrEmpty(Cvv))
                mensagens.Add($"O Cvv deve ser informado");

            if (string.IsNullOrEmpty(CardNumber))
                mensagens.Add($"O CardNumber deve ser informada");

            var apenasNumerosDoCartao = Helper.ApenasNumeros(CardNumber);

            if (string.IsNullOrEmpty(DueDate))
                mensagens.Add($"A DueDate deve ser informada");

            if (!string.IsNullOrEmpty(DueDate))
            {
                if (DueDate.Length != 7 || DueDate.Split('/').Length != 2)
                    mensagens.Add($"A DueDate deve ser informada no formato MM/YYYY");

                var refMes = Convert.ToInt32(Functions.Helper.ApenasNumeros(DueDate.Split('/')[0]));
                if (refMes < 1 || refMes > 12)
                    mensagens.Add("O mês deve ser informado com dois dígitos e deve estar entre 01 e 12");

                var refAno = Convert.ToInt32(Functions.Helper.ApenasNumeros(DueDate.Split('/')[1]));
                if (refAno < DateTime.Now.Year || refAno > 2099)
                    mensagens.Add($"O ano deve ser informado com 4 dígitos e deve estar entre {DateTime.Now.Year:yyyy} e 2099");
            }

            if (string.IsNullOrEmpty(Token))
                mensagens.Add($"O Token deve ser informado");

            if (string.IsNullOrEmpty(Token2))
                mensagens.Add($"O Token2 deve ser informado");

            if (string.IsNullOrEmpty(Status))
                mensagens.Add($"O Status deve ser informado");

            if (string.IsNullOrEmpty(CompanyId))
                mensagens.Add($"A CompanyId deve ser informada");

            if (string.IsNullOrEmpty(CompanyToken))
                mensagens.Add($"O CompanyToken deve ser informado");

            if (string.IsNullOrEmpty(Acquirer))
                mensagens.Add($"O Acquirer deve ser informado");

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
