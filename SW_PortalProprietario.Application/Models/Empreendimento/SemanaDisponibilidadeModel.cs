namespace SW_PortalProprietario.Application.Models.Empreendimento
{
    public class SemanaDisponibilidadeModel
    {
        public int Id { get; set; }
        public int SemanaId { get; set; }
        public DateTime? SemanaDataInicial { get; set; }
        public DateTime? SemanaDataFinal { get; set; }
        public int? TipoSemanaId { get; set; }
        public string? TipoSemanaNome { get; set; }
        public int? GrupoTipoSemanaId { get; set; }
        public string? GrupoTipoSemanaNome { get; set; }
        public int? UhCondominio { get; set; }
        public int? Capacidade { get; set; }
    }
}
