namespace SW_PortalProprietario.Application.Models.SystemModels
{
    public class RegraIntercambioInputModel
    {
        public int? TipoContratoId { get; set; }
        public string TipoSemanaCedida { get; set; } = string.Empty;
        public string TiposSemanaPermitidosUso { get; set; } = string.Empty;
        public DateTime DataInicioVigenciaCriacao { get; set; }
        public DateTime? DataFimVigenciaCriacao { get; set; }
        public DateTime DataInicioVigenciaUso { get; set; }
        public DateTime? DataFimVigenciaUso { get; set; }
    }
}
