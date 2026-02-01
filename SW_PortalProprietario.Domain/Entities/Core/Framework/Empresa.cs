using SW_PortalProprietario.Domain.Entities.Core.DadosPessoa;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Domain.Entities.Core.Framework
{
    public class Empresa : EntityBaseCore, IEntityValidateCore
    {
        public virtual GrupoEmpresa? GrupoEmpresa { get; set; }
        public virtual Pessoa? Pessoa { get; set; }
        public virtual string? Codigo { get; set; }
        public virtual string? NomeCondominio { get; set; }
        public virtual string? CnpjCondominio { get; set; }
        public virtual string? EnderecoCondominio { get; set; }
        public virtual string? NomeAdministradoraCondominio { get; set; }
        public virtual string? CnpjAdministradoraCondominio { get; set; }
        public virtual string? EnderecoAdministradoraCondominio { get; set; }


        public virtual async Task SaveValidate()
        {
            List<string> mensagens = new();


            if (Pessoa == null)
                mensagens.Add($"A Pessoa deve ser informada na Empresa");

            if (string.IsNullOrEmpty(Codigo))
                mensagens.Add($"O Código da Empresa deve ser informado");

            if (Pessoa?.TipoPessoa != EnumTipoPessoa.Juridica)
                mensagens.Add($"A Pessoa deve ser do tipo 1 - Jurídica");

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
