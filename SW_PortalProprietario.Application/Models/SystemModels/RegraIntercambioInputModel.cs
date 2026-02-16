namespace SW_PortalProprietario.Application.Models.SystemModels
{
    public class RegraIntercambioInputModel
    {
        public int? Id { get; set; }
        /// <summary>
        /// IDs dos tipos de contrato (separados por vírgula). Null/vazio = todos.
        /// </summary>
        public string? TipoContratoIds { get; set; }
        public string TipoSemanaCedida { get; set; } = string.Empty;
        public string TiposSemanaPermitidosUso { get; set; } = string.Empty;
        public DateTime DataInicioVigenciaCriacao { get; set; }
        public DateTime? DataFimVigenciaCriacao { get; set; }
        public DateTime DataInicioVigenciaUso { get; set; }
        public DateTime? DataFimVigenciaUso { get; set; }
        /// <summary>
        /// IDs dos tipos de UH permitidos (separados por vírgula). Null/vazio = todos.
        /// </summary>
        public string? TiposUhEsolIds { get; set; }
        public string? TiposUhCmIds { get; set; }
    }
}
