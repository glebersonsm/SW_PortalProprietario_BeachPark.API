using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Domain.Entities.Core.Framework
{
    public class Modulo : EntityBaseCore, IEntityValidateCore
    {
        public virtual AreaSistema? AreaSistema { get; set; }
        public virtual GrupoModulo? GrupoModulo { get; set; }
        public virtual string? Codigo { get; set; }
        public virtual string? Nome { get; set; }
        public virtual string? NomeInterno { get; set; }
        public virtual EnumStatus? Status { get; set; }

        public virtual List<int> Permissoes { get; set; } = new List<int>();

        public virtual async Task SaveValidate()
        {
            List<string> mensagens = new();

            if (AreaSistema == null)
                mensagens.Add("A Área do sistema deve ser informada no Módulo");

            if (GrupoModulo == null)
                mensagens.Add("O Grupo de Módulo deve ser informada no Módulo");

            if (string.IsNullOrEmpty(Codigo))
                mensagens.Add($"O Código do Módulo deve ser informado");

            if (string.IsNullOrEmpty(Nome))
                mensagens.Add($"O Mome do Módulo deve ser informado");

            if (string.IsNullOrEmpty(NomeInterno))
                mensagens.Add($"O Mome Interno do Módulo deve ser informado");

            if (!Status.HasValue)
                mensagens.Add($"O Status do Módulo deve ser informado");

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
