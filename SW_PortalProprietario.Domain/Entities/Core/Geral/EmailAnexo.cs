using SW_PortalProprietario.Domain.Entities.Core;

namespace SW_PortalProprietario.Domain.Entities.Core.Geral
{
    public class EmailAnexo : EntityBaseCore
    {
        public virtual Email? Email { get; set; }
        public virtual string NomeArquivo { get; set; } = string.Empty;
        public virtual string TipoMime { get; set; } = "application/pdf";
        public virtual byte[] Arquivo { get; set; } = Array.Empty<byte>();
    }
}

