namespace CMDomain.Models.Financeiro
{

    public class RetornoIntegracaoNotaRequestiaInputModel
    {
        public string? UsuarioLogado { get; set; }
        public List<RetornoIntegracaoNotaRequestiaInputItemModel> Itens { get; set; } = new List<RetornoIntegracaoNotaRequestiaInputItemModel>();

    }
    public class RetornoIntegracaoNotaRequestiaInputItemModel
    {
        public int? RecebimentoMercadoriaId { get; set; }
        public string? ChaveIntegracaoRequestia { get; set; }
        public int? Processado { get; set; }
        public string? ResultGravacaoCm { get; set; }

    }
}
