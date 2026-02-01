using SW_PortalProprietario.Domain.Functions;

namespace SW_PortalProprietario.Domain.Entities.Core.Geral
{
    public class Cidade : EntityBaseCore, IEntityValidateCore
    {
        public virtual Estado? Estado { get; set; }
        public virtual string? Nome { get; set; }
        public virtual string? CodigoIbge { get; set; }
        public virtual string? NomePesquisa { get; set; }

        public virtual async Task SaveValidate()
        {
            List<string> mensagens = new();

            if (Estado == null)
                mensagens.Add("O Estado deve ser informado na Cidade");

            if (string.IsNullOrEmpty(Nome))
                mensagens.Add($"O Nome da Cidade deve ser informado");

            if (string.IsNullOrEmpty(CodigoIbge))
                mensagens.Add($"O Código da Cidade no IBGE deve ser informado");

            if (!string.IsNullOrEmpty(Nome))
                NomePesquisa = Nome.ToUpper().RemoveAccentsFromDomain();

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
