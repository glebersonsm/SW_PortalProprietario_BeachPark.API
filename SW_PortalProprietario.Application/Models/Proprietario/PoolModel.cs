namespace SW_PortalProprietario.Application.Models.Proprietario
{
    public class PoolModel
    {
        public int Id { get; set; }
        public DateTime? DataHoraCriacao { get; set; }
        public int? Filial { get; set; }
        public string? Codigo { get; set; }
        public string? Nome { get; set; }
        public int? FilialEmpreendimento { get; set; }
        public int? Empreendimento { get; set; }
        public string? TipoRateio { get; set; }
        public int? TipoCliente { get; set; }
        public int? Empresa { get; set; }
        public string? UtilizaIntegracaoPortal { get; set; }
        public string? UrlApiPortal { get; set; }
        public string? ChaveAcessoApiPortal { get; set; }

    }
}
