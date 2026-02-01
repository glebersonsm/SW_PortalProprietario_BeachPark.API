using SW_PortalProprietario.Domain.Entities.Core.Framework;

namespace SW_PortalProprietario.Domain.Entities.Core.Geral
{
    public class ContratoVinculoSCPEsol : EntityBaseCore, IEntityValidateCore
    {
        public virtual Empresa? Empresa { get; set; }
        public virtual int? PessoaLegadoId { get; set; }
        public virtual int? CotaPortalId { get; set; }
        public virtual int? CotaAccessCenterId { get; set; }
        public virtual int? UhCondominioId { get; set; }
        public virtual string? DadosQualificacaoCliente { get; set; }
        public virtual string? CodigoVerificacao { get; set; }
        public virtual string? DocumentoFull { get; set; }
        public virtual string? PdfPath { get; set; }
        public virtual int? Idioma { get; set; } = 0;
        public virtual async Task SaveValidate()
        {
            List<string> mensagens = new();

            if (Empresa == null)
                mensagens.Add("A Empresa deve ser informada");

            if (PessoaLegadoId.GetValueOrDefault(0) <= 0)
                mensagens.Add($"A PessoaLegadoId deve se informada.");

            if (CotaPortalId.GetValueOrDefault(0) == 0)
                mensagens.Add($"A CotaId deve ser informada.");

            if (CotaAccessCenterId.GetValueOrDefault(0) == 0)
                mensagens.Add($"A CotaAccessCenterId deve ser informada.");

            if (UhCondominioId.GetValueOrDefault(0) == 0)
                mensagens.Add($"A UhCondominioId deve ser informada.");

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
