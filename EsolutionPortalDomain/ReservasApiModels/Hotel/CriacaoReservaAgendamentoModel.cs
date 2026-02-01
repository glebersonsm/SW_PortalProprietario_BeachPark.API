using EsolutionPortalDomain.Enums;

namespace EsolutionPortalDomain.ReservasApiModels.Hotel
{
    public class CriacaoReservaAgendamentoModel
    {
        public int Id { get; set; }
        public EnumReservaStatus Status { get; set; } = EnumReservaStatus.AC;
        public EnumTipoTarifacao TipoTarifacao { get; set; } = EnumTipoTarifacao.DiaDia;
        public EnumReservaTipoPensao TipoPensao { get; set; } = EnumReservaTipoPensao.NN;

        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public DateTime DataHora { get; set; }
        public DateTime? DataHoraAConfirmar { get; set; }
        public DateTime? DataHoraConfirmacao { get; set; }
        public DateTime? DataHoraCancelamento { get; set; }

        public int QuantidadeAdultos { get; set; }
        public int QuantidadeCrianca1 { get; set; }
        public int QuantidadeCrianca2 { get; set; }

        public int AgendamentoId { get; set; }
        public List<HospedesReservaAgendamentoModel>? Hospedes { get; set; }
    }


}
