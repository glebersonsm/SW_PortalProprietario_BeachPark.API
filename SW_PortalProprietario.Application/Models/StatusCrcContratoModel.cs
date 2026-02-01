namespace SW_PortalProprietario.Application.Models
{
    public class StatusCrcContratoModel
    {
        public DateTime? AtendimentoStatusCrcData { get; set; }
        public int? FrAtendimentoVendaId { get; set; }
        public int? IdVendaXContrato { get; set; }
        public int? AtendimentoStatusCrcId { get; set; }
        public string? AtendimentoStatusCrcStatus { get; set; }
        public string? CodigoStatus { get; set; }
        public string? NomeStatus { get; set; }
        public string? BloqueaRemissaoBoletos { get; set; }
        public string? BloquearCobrancaPagRec { get; set; }
        public string? BloqueaCobrancaEmail { get; set; }
        public string? FrStatusCrcId { get; set; }
        public string? FrCrcStatus { get; set; }
        public string? BloqueiaUtilizacaoCota { get; set; }
        public string? NomeTitular { get; set; }
        public string? Cpf_Cnpj_Titular { get; set; }
        public int? IdPessoa { get; set; }
        public string? NomeCoCessionario { get; set; }
        public string? Cpf_Cnpj_CoCessionario { get; set; }
        public string? ImovelNumero { get; set; }
        public string? GrupoCotaTipoCotaCodigo { get; set; }
        public string? GrupoCotaTipoCotaNome { get; set; }
        public string? PadraoDeCor { get; set; } = "Default";

    }
}
