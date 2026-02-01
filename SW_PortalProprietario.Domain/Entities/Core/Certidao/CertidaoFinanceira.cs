using SW_PortalProprietario.Domain.Entities.Core.DadosPessoa;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Domain.Entities.Core.Certidao
{
    public class CertidaoFinanceira : EntityBaseCore, IEntityValidateCore
    {
        public virtual Pessoa? Pessoa { get; set; }
        public virtual string? Protocolo { get; set; }
        public virtual string? Conteudo { get; set; }
        public virtual DateTime? CompetenciaInicial { get; set; }
        public virtual DateTime? CompetenciaFinal { get; set; }
        public virtual EnumCertidaoTipo? Tipo { get; set; }
        public virtual string? ImovelNumero { get; set; }
        public virtual string? NumeroFracao { get; set; }
        public virtual string? UrlValidacaoProtocolo { get; set; }
        public virtual string? PdfPath { get; set; }
        public virtual string? Competencia { get; set; }
        public virtual string? MultiProprietario { get; set; }
        public virtual string? NomeCampoCpfCnpj { get; set; }
        public virtual string? CpfCnpj { get; set; }
        public virtual string? TorreBlocoNome { get; set; }
        public virtual string? TorreBlocoNumero { get; set; }
        public virtual string? CertidaoEmitidaEm { get; set; }
        public virtual DateTime? Data { get; set; }

        public virtual async Task SaveValidate()
        {
            List<string> mensagens = new();

            if (Pessoa == null)
                mensagens.Add($"A Pessoa deve ser informada");

            if (Id > 0)
            {
                if (string.IsNullOrEmpty(Protocolo))
                    mensagens.Add($"O Protocolo deve ser informado");

                if (string.IsNullOrEmpty(Conteudo))
                    mensagens.Add($"O Conteúdo deve ser informado");

            }

            if (!Tipo.HasValue)
            {
                mensagens.Add($"O Tipo da certidão deve ser informado");
            }

            if (CompetenciaInicial.GetValueOrDefault(DateTime.MinValue) == DateTime.MinValue)
                mensagens.Add($"Deve ser informada a Competência inicial");

            if (CompetenciaFinal.GetValueOrDefault(DateTime.MinValue) == DateTime.MinValue)
                mensagens.Add($"Deve ser informada a Competência final");

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
