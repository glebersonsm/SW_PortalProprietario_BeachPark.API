namespace SW_PortalProprietario.Application.Models.GeralModels
{
    public class EmailAnexoInputModel
    {
        public string NomeArquivo { get; set; } = string.Empty;
        public string TipoMime { get; set; } = "application/pdf";
        public byte[] Arquivo { get; set; } = Array.Empty<byte>();

    }

}
