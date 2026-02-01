using System;
using System.Collections.Generic;

namespace SW_PortalProprietario.Application.Models.TimeSharing
{
    /// <summary>
    /// Modelo de request para cálculo simplificado de pontos necessários
    /// </summary>
    public class CalcularPontosRequestModel
    {
        /// <summary>
        /// Data inicial (checkin) da reserva
        /// </summary>
        public DateTime DataInicial { get; set; }

        /// <summary>
        /// Data final (checkout) da reserva
        /// </summary>
        public DateTime DataFinal { get; set; }

        /// <summary>
        /// Quantidade de adultos (12 anos ou mais)
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
        /// ID da venda x contrato
        /// </summary>
        public int IdVendaXContrato { get; set; }

        /// <summary>
        /// Número do contrato
        /// </summary>
        public string NumeroContrato { get; set; } = string.Empty;
        public int? NumReserva { get; set; }

    }
}
