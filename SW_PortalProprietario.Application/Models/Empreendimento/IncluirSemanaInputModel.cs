using EsolutionPortalDomain.Portal;

namespace SW_PortalProprietario.Application.Models.Empreendimento
{
    public class IncluirSemanaInputModel : LiberacaoMeuAgendamentoInputModel
    {
        public int? CotaAcId { get; set; }
        public int? CotaId { get; set; }
        public int? SemanaId { get; set; }
        public int? UhCondominioId { get; set; }
        public string? CotaPortalNome { get; set; }
        public string? CotaPortalCodigo { get; set; }
        public string? GrupoCotaPortalNome { get; set; }
        public string? NumeroImovel { get; set; }
        public int? CotaProprietarioId { get; set; }
        public int? EmpresaPortalId { get; set; }
        public int? EmpresaAcId { get; set; }
        public string? IdIntercambiadora { get; set; }
        public string? TipoUso { get; set; }
        public string? TipoUtilizacao { get; set; }
        public bool? AdmAsUser { get; set; }
    }
}
