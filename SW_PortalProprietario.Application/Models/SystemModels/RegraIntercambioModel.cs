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
        /// <summary>IDs dos tipos de semana cedida (eSolution), separados por vírgula. Vazio = todos.</summary>
        public string TipoSemanaCedida { get; set; } = string.Empty;
        /// <summary>Nomes para exibição (resolvidos a partir dos IDs).</summary>
        public string? TipoSemanaCedidaNome { get; set; }
        /// <summary>IDs dos tipos de semana permitidos para uso (CM), separados por vírgula. Vazio = todos.</summary>
        public string TiposSemanaPermitidosUso { get; set; } = string.Empty;
        /// <summary>Nomes para exibição (resolvidos a partir dos IDs).</summary>
        public string? TiposSemanaPermitidosUsoNomes { get; set; }
        public DateTime DataInicioVigenciaCriacao { get; set; }
        public DateTime? DataFimVigenciaCriacao { get; set; }
        public DateTime DataInicioVigenciaUso { get; set; }
        public DateTime? DataFimVigenciaUso { get; set; }
        public string? TiposUhEsolIds { get; set; }
        public string? TiposUhCmIds { get; set; }
        public string? TiposUhNomes { get; set; }
    }
}
