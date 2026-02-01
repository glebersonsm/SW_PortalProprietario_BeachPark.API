namespace SW_PortalProprietario.Application.Models.Empreendimento
{
    public class DispobilidadeSearchModel
    {
        public int? Agendamentoid { get; set; }
        public int? CotaAccessCenterId { get; set; }
        public int? CotaPortalId { get; set; }
        public int? UhCondominioId { get; set; }
        public string? CotaPortalNome { get; set; }
        public string? CotaPortalCodigo { get; set; }
        public string? GrupoCotaPortalNome { get; set; }
        public string? NumeroImovel { get; set; }
        public int? CotaProprietarioId { get; set; }
        public int? Ano { get; set; }
        public int? EmpresaAcId { get; set; }
        public int? EmpresaPortalId { get; set; }
        public bool? AdmVisaoDeCliente { get; set; } = false;
        public int? PessoaLegadoId { get; set; }

    }
}
