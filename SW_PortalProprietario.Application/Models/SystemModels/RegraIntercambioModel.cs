using SW_PortalProprietario.Application.Models;

namespace SW_PortalProprietario.Application.Models.SystemModels
{
    public class RegraIntercambioModel : ModelBase
    {
        /// <summary>
        /// IDs dos tipos de contrato (separados por vírgula). Null/vazio = todos.
        /// </summary>
        public string? TipoContratoIds { get; set; }
        public string? TipoContratoNome { get; set; }
        public string TipoSemanaCedida { get; set; } = string.Empty;
        public string TiposSemanaPermitidosUso { get; set; } = string.Empty;
        public DateTime DataInicioVigenciaCriacao { get; set; }
        public DateTime? DataFimVigenciaCriacao { get; set; }
        public DateTime DataInicioVigenciaUso { get; set; }
        public DateTime? DataFimVigenciaUso { get; set; }
        public string? TiposUhIds { get; set; }
        public string? TiposUhNomes { get; set; }
    }
}
