using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Domain.Entities.Core.Framework
{
    public class Permissao : EntityBaseCore, IEntityValidateCore
    {
        public virtual string? Nome { get; set; }
        public virtual string? NomeInterno { get; set; }
        public virtual EnumSimNao? UsarNomeInterno { get; set; }
        public virtual string? TipoPermissao { get; set; } //R = Read, W = Write, D = Delete, Full = *, 

        public virtual async Task SaveValidate()
        {
            List<string> mensagens = new();


            if (string.IsNullOrEmpty(Nome))
                mensagens.Add($"O Nome da Permissão deve ser informado");

            if (string.IsNullOrEmpty(NomeInterno))
                mensagens.Add($"O Nome Interno da Permissão deve ser informado");

            if (!UsarNomeInterno.HasValue)
                mensagens.Add($"O campo Uar Nome Interno da Permissão deve ser informado na Permissão");

            if (string.IsNullOrEmpty(TipoPermissao) || !new List<string>() { "R", "W", "D", "*", "Download" }.Any(b => b == TipoPermissao))
                mensagens.Add($"O Tipo de Prmissão deve ser informado na Permissão, valores válidos: (R = Read, W = Write, D = Delete, Full = *, Download)");

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
