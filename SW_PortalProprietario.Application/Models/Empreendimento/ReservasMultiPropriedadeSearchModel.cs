using System.Security.Policy;

namespace SW_PortalProprietario.Application.Models.Empreendimento
{
    public class ReservasMultiPropriedadeSearchModel : PaginatedSearchModel
    {
        public int? PeriodoCotaDisponibilidadeId { get; set; }
        public string? DocumentoProprietario { get; set; }
        public string? NomeProprietario { get; set; }
        public string? NumeroApartamento { get; set; }
        public int? Ano { get; set; }
        public DateTime? DataUtilizacaoInicial { get; set; }
        public DateTime? DataUtilizacaoFinal { get; set; }
        public string? ComReservas { get; set; }
        public string? NomeCota { get; set; }
        public int? Reserva { get; set; }
        public bool? ApenasInadimplentes { get; set; }
        public bool? NaoConsiderarParcelasCondominio { get; set; }
        public bool? NaoConsiderarParcelasContrato { get; set; }
        public List<int>? StatusCrcIds { get; set; }
        public List<string>? NumeroApartamentos { get; set; }
        public List<string>? NomeCotas { get; set; }

    }
}
