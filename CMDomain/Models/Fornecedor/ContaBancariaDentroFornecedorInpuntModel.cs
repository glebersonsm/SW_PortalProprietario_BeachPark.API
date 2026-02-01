namespace CMDomain.Models.Fornecedor
{
    public class ContaBancariaDentroFornecedorInputModel : ModelRequestBase
    {
        public int? IdContaBancaria { get; set; }
        public BancoInputModel? BancoInputModel { get; set; }
        public AgenciaBancariaInputModel? AgenciaBancariaInputModel { get; set; }
        public int? IdFornecedor { get; set; }
        public string? NumeroConta { get; set; }
        public int? TipoConta { get; set; } //1 = Corrente, 2 = Salário, 3 = Poupança
        public string? Preferencial { get; set; }
        public string? Ativa { get; set; }
        public int? IdContaBancariaXChavePix { get; set; }
        public string? ChavePix { get; set; }
        public string? ChavePixPreferencial { get; set; }
        public int? IdTipoChave { get; set; }

    }
}
