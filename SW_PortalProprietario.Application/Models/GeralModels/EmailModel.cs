using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Models.GeralModels
{
    public class EmailModel : ModelBase
    {
        public int? EmpresaId { get; set; }
        public string? Assunto { get; set; }
        public string? Destinatario { get; set; }
        public string? ConteudoEmail { get; set; }
        public EnumSimNao? Enviado { get; set; }
        public DateTime? DataHoraPrimeiraAbertura { get; set; }
        public List<EmailAnexoModel>? Anexos { get; set; }
    }

    public class EmailAnexoModel
    {
        public int Id { get; set; }
        public string NomeArquivo { get; set; } = string.Empty;
        public string TipoMime { get; set; } = "application/pdf";
        public byte[] Arquivo { get; set; } = Array.Empty<byte>();
    }
}
