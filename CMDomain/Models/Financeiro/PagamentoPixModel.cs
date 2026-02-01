namespace CMDomain.Models.Financeiro
{
    public class PagamentoPixModel
    {
        public int? IdContaXChave { get; set; }
        public int? IdCBancaria { get; set; }
        public string? ContaCorrente { get; set; }
        public string? AgenciaBancaria { get; set; }
        public string? Beneficiario { get; set; }
        public string? ChavePix { get; set; }

    }
}
