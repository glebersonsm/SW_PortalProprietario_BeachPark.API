namespace CMDomain.Models.Compras
{
    public class RemoverListaPrecoResultModel
    {
        public bool? PrecosRemovidos { get; set; }
        public List<string> Erros { get; set; } = new List<string>();
    }
}
