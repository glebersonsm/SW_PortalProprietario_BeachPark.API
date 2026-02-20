namespace SW_PortalProprietario.Domain.Entities.Core.Geral
{
    /// <summary>
    /// Regra de intercÃ¢mbio: define quais tipos de semana podem ser utilizados ao ceder um tipo especÃ­fico.
    /// Inclui perÃ­odos de vigÃªncia para criaÃ§Ã£o e utilizaÃ§Ã£o da reserva.
    /// </summary>
    public class RegraIntercambio : EntityBaseCore
    {
        /// <summary>
        /// IDs dos tipos de contrato (eSolution), separados por vÃ­rgula. Null/vazio = todos.
        /// </summary>
        public virtual string? TipoContratoIds { get; set; }
        public virtual string TipoSemanaCedida { get; set; } = string.Empty;
        public virtual string TiposSemanaPermitidosUso { get; set; } = string.Empty;
        public virtual DateTime DataInicioVigenciaCriacao { get; set; }
        public virtual DateTime? DataFimVigenciaCriacao { get; set; }
        public virtual DateTime DataInicioVigenciaUso { get; set; }
        public virtual DateTime? DataFimVigenciaUso { get; set; }
        /// <summary>
        /// IDs dos tipos de UH (TipoUh do eSolution), separados por vÃ­rgula. Null/vazio = todos.
        /// </summary>
        public virtual string? TiposUhEsolIds { get; set; }
        /// <summary>
        /// IDs dos tipos de UH (TipoUh do CM), separados por vÃ­rgula. Null/vazio = todos.
        /// </summary>
        public virtual string? TiposUhCmIds { get; set; }
    }
}
