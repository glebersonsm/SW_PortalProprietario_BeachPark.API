namespace EsolutionPortalDomain.Portal
{
    public class LiberacaoMeuAgendamentoInputModel
    {
        public int? AgendamentoId { get; set; }
        public int? InventarioId { get; set; }
        public int? ClienteContaBancariaId { get; set; }
        public string? CodigoVerificacao { get; set; }
        public string? CodigoBanco { get; set; }
        public string? Agencia { get; set; }
        public string? AgenciaDigito { get; set; }
        public string? ContaNumero { get; set; }
        public string? Variacao { get; set; }
        public string? ContaDigito { get; set; }
        public bool? Preferencial { get; set; } = false;
        public int? IdCidade { get; set; }
        public string? TipoChavePix { get; set; }
        public string? ChavePix { get; set; }
        public string? Status { get; set; }
        public string? DocumentoTitularConta { get; set; }
        public int? CotaAccessCenterId { get; set; }

    }
}
