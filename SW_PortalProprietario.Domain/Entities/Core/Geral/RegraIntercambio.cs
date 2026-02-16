namespace SW_PortalProprietario.Domain.Entities.Core.Geral
{
    /// <summary>
    /// Regra de intercâmbio: define quais tipos de semana podem ser utilizados ao ceder um tipo específico.
    /// Inclui períodos de vigência para criação e utilização da reserva.
    /// </summary>
    public class RegraIntercambio : EntityBaseCore
    {
        /// <summary>
        /// IDs dos tipos de contrato (eSolution), separados por vírgula. Null/vazio = todos.
        /// </summary>
        public virtual string? TipoContratoIds { get; set; }
        public virtual string TipoSemanaCedida { get; set; } = string.Empty;
        public virtual string TiposSemanaPermitidosUso { get; set; } = string.Empty;
        public virtual DateTime DataInicioVigenciaCriacao { get; set; }
        public virtual DateTime? DataFimVigenciaCriacao { get; set; }
        public virtual DateTime DataInicioVigenciaUso { get; set; }
        public virtual DateTime? DataFimVigenciaUso { get; set; }
        /// <summary>
        /// IDs dos tipos de UH (TipoUh do eSolution), separados por vírgula. Null/vazio = todos.
        /// </summary>
        public virtual string? TiposUhEsolIds { get; set; }
        /// <summary>
        /// IDs dos tipos de UH (TipoUh do CM), separados por vírgula. Null/vazio = todos.
        /// </summary>
        public virtual string? TiposUhCmIds { get; set; }
    }
}
