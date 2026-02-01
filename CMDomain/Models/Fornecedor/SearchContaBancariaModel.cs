namespace CMDomain.Models.Fornecedor
{
    public class SearchContaBancariaModel
    {
        public string? UsuarioLogado { get; set; }
        public int? IdFornecedor { get; set; }
        public int? IdContaBancaria { get; set; }
        public int? IdAgenciaBancaria { get; set; }
        public string? NomeBanco { get; set; }
        public string? NumBanco { get; set; }
        public string? NumeroConta { get; set; }
        public string? ChavePix { get; set; }

    }
}
