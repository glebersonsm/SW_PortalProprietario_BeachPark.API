namespace SW_PortalProprietario.Application.Models.TimeSharing
{
    public class SearchDisponibilidadeParaTrocaModel
    {
        public int? ReservaId { get; set; }
        public string? NumeroContrato { get; set; }
        public int? IdVendaXContrato { get; set; }
        public string? HotelId { get; set; }
        public DateTime? DataInicial { get; set; }
        public DateTime? DataFinal { get; set; }
        public string? TipoDeBusca { get; set; }
        
        /// <summary>
        /// Quantidade de adultos da reserva atual (para cÃ¡lculo correto dos pontos)
        /// </summary>
        public int? QuantidadeAdultos { get; set; }
        
        /// <summary>
        /// Quantidade de crianÃ§as de 6-11 anos da reserva atual
        /// </summary>
        public int? QuantidadeCriancas1 { get; set; }
        
        /// <summary>
        /// Quantidade de crianÃ§as de 0-5 anos da reserva atual
        /// </summary>
        public int? QuantidadeCriancas2 { get; set; }
    }
}
