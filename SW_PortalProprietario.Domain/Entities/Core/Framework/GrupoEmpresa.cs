using SW_PortalProprietario.Domain.Entities.Core.DadosPessoa;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Domain.Entities.Core.Framework
{
    public class GrupoEmpresa : EntityBaseCore, IEntityValidateCore
    {
        public virtual Pessoa? Pessoa { get; set; }
        public virtual string? Codigo { get; set; }
        public virtual EnumStatus? Status { get; set; } = EnumStatus.Ativo;

        public virtual async Task SaveValidate()
        {
            List<string> mensagens = new();


            if (Pessoa == null)
                mensagens.Add($"A Pessoa deve ser informada no Grupo de Empresa");

            if (string.IsNullOrEmpty(Codigo))
                mensagens.Add($"O Código do Grupo de Empresa deve ser informado");

            if (Pessoa?.TipoPessoa != EnumTipoPessoa.Juridica)
                mensagens.Add($"A Pessoa deve ser do tipo 1 - Jurídica");

            if (!Status.HasValue)
                mensagens.Add($"O Status do Grupo de Empresa deve ser informado");


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
