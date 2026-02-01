namespace CMDomain.Models.Compras
{
    public class GravacaoListaPrecoResultModel
    {
        public bool? ListaGravada { get; set; }
        public List<string> Erros { get; set; } = new List<string>();
    }
}
