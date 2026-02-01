namespace SW_PortalProprietario.Application.Models.Empreendimento
{
    public class ImovelSimplificadoModel
    {
        public int Id { get; set; }
        public DateTime? DataCriacao { get; set; }
        public int? EmpreendimentoId { get; set; }
        public string? EmpreendimentoNome { get; set; }
        public string? ImovelNumero { get; set; }
        public string? BlocoCodigo { get; set; }
        public string? BlocoNome { get; set; }
        public string? ImovelAndarCodigo { get; set; }
        public string? ImovelAndarNome { get; set; }
        public string? TipoImovelCodigo { get; set; }
        public string? TipoImovelNome { get; set; }
        public int? QtdeVendida { get; set; }
        public int? QtdeDisponivel { get; set; }
        public int? QtdeBloqueada { get; set; }

    }
}
