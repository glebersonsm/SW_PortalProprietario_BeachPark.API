namespace AccessCenterDomain.AccessCenter.Model
{
    public class ContaReceberParcelaVinculacaoCartaoRecorrenteModel
    {
        public int Id { get; set; }
        public int ContaReceberId { get; set; }
        public int ClienteId { get; set; }
        public string Status { get; set; }
        public string Tag { get; set; }
        public string IntegracaoId { get; set; }
        public int? ClienteCartaoCredito { get; set; }
        public string CartaoCreditoRecorrenteStatus { get; set; } = null;

    }
}
