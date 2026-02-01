namespace SW_PortalProprietario.Application.Models
{
    public class VinculoAccessXPortalBase
    {
        public int? AcCotaId { get; set; }
        public int? EsolCotaId { get; set; }
        public string? AcCotaNome { get; set; }
        public string? EsolCotaNome { get; set; }
        public string? AcNumeroImovel { get; set; }
        public string? EsolNumeroImovel { get; set; }
        public string? AcEmpreendimentoNome { get; set; }
        public int? AcPessoaProprietarioId { get; set; }
        public int? EsolPessoaProprietarioId { get; set; }
        public string? AcPessoaProprietarioNome { get; set; }
        public string? EsolPessoaProprietarioNome { get; set; }
        public int? EmpreendimentoId { get; set; }
        public string? PadraoDeCor { get; set; } = "Default";

    }
}
