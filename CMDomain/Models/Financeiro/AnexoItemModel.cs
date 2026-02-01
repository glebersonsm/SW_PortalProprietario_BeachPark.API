namespace CMDomain.Models.Financeiro
{
    public class AnexoItemModel
    {
        public int? IdArquivoAnexo { get; set; }
        public int? IdArquivo { get; set; }
        public string? Nome { get; set; }
        public byte[]? Conteudo { get; set; }
    }
}
