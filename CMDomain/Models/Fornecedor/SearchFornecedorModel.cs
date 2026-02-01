namespace CMDomain.Models.Fornecedor
{
    public class SearchFornecedorModel
    {
        public string? UsuarioLogado { get; set; }
        public int? IdFornecedor { get; set; }
        public int? IdEmpresa { get; set; }
        public string? Nome { get; set; }
        public string? Documento { get; set; }
        public bool ApenasAtivos { get; set; } = true;
        public List<int>? IdsRamosDoFornecedor { get; set; }

    }
}
