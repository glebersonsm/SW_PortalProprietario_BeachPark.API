namespace EsolutionPortalDomain.ReservasApiModels
{
    public class LoggedUserModel
    {
        public int? UserPortalSwId { get; set; }
        public int? PessoaACId { get; set; }
        public int? EmpresaACId { get; set; }
        public int? CotaAcId { get; set; }
        public int? CotaPortalId { get; set; }
        public string? CotaPortalNome { get; set; }
        public string? CotaPortalNumeroImovel { get; set; }
        public int? CotaPortalPessoaProprietarioId { get; set; }
    }
}
