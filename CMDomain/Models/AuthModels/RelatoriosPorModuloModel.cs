namespace CMDomain.Models.AuthModels
{
    public class RelatoriosPorModuloModel
    {
        public string? ModuloId { get; set; }
        public string? ModuloNome { get; set; }
        public int? GrupoRelatorioId { get; set; }
        public string? GrupoRelatorioNome { get; set; }
        public int? RelatorioId { get; set; }
        public string? RelatorioNome { get; set; }
        public bool? OrigemCm { get; set; }

    }
}
