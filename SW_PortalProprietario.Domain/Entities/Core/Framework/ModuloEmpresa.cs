using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Domain.Entities.Core.Framework
{
    public class ModuloEmpresa : EntityBaseCore, IEntityValidateCore
    {
        public virtual Empresa? Empresa { get; set; }
        public virtual Modulo? Modulo { get; set; }
        public virtual EnumStatus? Status { get; set; } = EnumStatus.Ativo;

        public virtual async Task SaveValidate()
        {
            List<string> mensagens = new();


            if (Empresa == null)
                mensagens.Add($"A Empresa deve ser informada no Módulo Empresa");

            if (Modulo == null)
                mensagens.Add($"O Módulo deve ser informado no Módulo Empresa");

            if (!Status.HasValue)
                mensagens.Add($"O Status do Módulo Empresa deve ser informado");


            if (mensagens.Any())
                await Task.FromException(new ArgumentException(string.Join($"{Environment.NewLine}", mensagens.Select(a => a).ToList())));
            else await Task.CompletedTask;
        }
    }
}
