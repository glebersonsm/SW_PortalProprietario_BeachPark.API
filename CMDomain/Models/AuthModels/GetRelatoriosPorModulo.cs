namespace CMDomain.Models.AuthModels
{
    public class GetRelatoriosPorModulo : ModelRequestBase
    {
        public int? ModuloId { get; set; }
        public string? ModuloNome { get; set; }
        public int? RelatorioId { get; set; }
        public string? RelatorioNome { get; set; }

    }
}
