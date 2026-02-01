using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Domain.Entities.Core.Geral
{
    public class DocumentTemplate : EntityBaseCore, IEntityValidateCore
    {
        public virtual EnumDocumentTemplateType TemplateType { get; set; }
        public virtual string? Name { get; set; }
        public virtual int Version { get; set; }
        public virtual string ContentHtml { get; set; } = string.Empty;
        public virtual bool Active { get; set; } = true;

        public virtual async Task SaveValidate()
        {
            List<string> mensagens = new();

            if (string.IsNullOrWhiteSpace(Name))
                mensagens.Add("O nome do template deve ser informado.");

            if (Active && string.IsNullOrWhiteSpace(ContentHtml))
                mensagens.Add("O conteúdo HTML do template deve ser informado.");

            if (Version <= 0)
                mensagens.Add("A versão deve ser maior que zero.");

            if (mensagens.Any())
                await Task.FromException(new ArgumentException(string.Join(Environment.NewLine, mensagens)));
            else
                await Task.CompletedTask;
        }
    }
}

