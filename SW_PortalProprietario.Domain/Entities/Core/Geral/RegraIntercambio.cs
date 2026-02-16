namespace SW_PortalProprietario.Domain.Entities.Core.Geral
{
    /// <summary>
    /// Regra de intercâmbio: define quais tipos de semana podem ser utilizados ao ceder um tipo específico.
    /// Inclui períodos de vigência para criação e utilização da reserva.
    /// </summary>
    public class RegraIntercambio : EntityBaseCore
    {
        public virtual int? TipoContratoId { get; set; }
        public virtual string TipoSemanaCedida { get; set; } = string.Empty;
        public virtual string TiposSemanaPermitidosUso { get; set; } = string.Empty;
        public virtual DateTime DataInicioVigenciaCriacao { get; set; }
        public virtual DateTime DataFimVigenciaCriacao { get; set; }
        public virtual DateTime DataInicioVigenciaUso { get; set; }
        public virtual DateTime DataFimVigenciaUso { get; set; }
    }
}
