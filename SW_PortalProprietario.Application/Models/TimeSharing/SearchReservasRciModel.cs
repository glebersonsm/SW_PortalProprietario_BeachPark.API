namespace SW_PortalProprietario.Application.Models.TimeSharing
{
    public class SearchReservasRciModel
    {
        public string? NomeCliente { get; set; }
        public string? NumeroContrato { get; set; }
        public DateTime? DataCriacaoInicial { get; set; }
        public DateTime? DataCriacaoFinal { get; set; }
        public string? UsuarioVinculacao { get; set; }
        public string? StatusCM { get; set; } // "Pendente" ou "Vinculada"
        public int NumeroDaPagina { get; set; } = 1;
        public int QuantidadeRegistrosRetornar { get; set; } = 15;
    }
}

