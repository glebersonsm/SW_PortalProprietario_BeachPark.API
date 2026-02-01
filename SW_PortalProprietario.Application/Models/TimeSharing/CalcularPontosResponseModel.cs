using System;

namespace SW_PortalProprietario.Application.Models.TimeSharing
{
    /// <summary>
    /// Modelo de response para cálculo simplificado de pontos necessários
    /// </summary>
    public class CalcularPontosResponseModel
    {
        /// <summary>
        /// Pontos necessários para o período solicitado
        /// </summary>
        public decimal PontosNecessarios { get; set; }

        /// <summary>
        /// Data inicial (checkin)
        /// </summary>
        public DateTime DataInicial { get; set; }

        /// <summary>
        /// Data final (checkout)
        /// </summary>
        public DateTime DataFinal { get; set; }

        /// <summary>
        /// Quantidade de diárias
        /// </summary>
        public int Diarias { get; set; }

        /// <summary>
        /// Total de hóspedes
        /// </summary>
        public int TotalHospedes { get; set; }

        /// <summary>
        /// Quantidade de adultos
        /// </summary>
        public int QuantidadeAdultos { get; set; }

        /// <summary>
        /// Quantidade de crianças de 6 a 11 anos
        /// </summary>
        public int QuantidadeCriancas1 { get; set; }

        /// <summary>
        /// Quantidade de crianças de 0 a 5 anos
        /// </summary>
        public int QuantidadeCriancas2 { get; set; }

        /// <summary>
        /// ID do hotel
        /// </summary>
        public int HotelId { get; set; }

        /// <summary>
        /// Nome do hotel
        /// </summary>
        public string? NomeHotel { get; set; }

        /// <summary>
        /// Tipo de apartamento
        /// </summary>
        public string? TipoApartamento { get; set; }

        /// <summary>
        /// Padrão tarifário aplicado
        /// </summary>
        public string? PadraoTarifario { get; set; }

        /// <summary>
        /// Número do contrato
        /// </summary>
        public string? NumeroContrato { get; set; }
    }
}
