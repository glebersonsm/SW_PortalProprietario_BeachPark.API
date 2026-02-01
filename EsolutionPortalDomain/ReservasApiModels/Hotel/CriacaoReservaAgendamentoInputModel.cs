namespace EsolutionPortalDomain.ReservasApiModels.Hotel
{
    public class CriacaoReservaAgendamentoInputModel
    {
        public int Id { get; set; }
        public string? Status { get; set; } = "AC";
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public int QuantidadeAdultos { get; set; }
        public int QuantidadeCrianca1 { get; set; }
        public int QuantidadeCrianca2 { get; set; }
        public int AgendamentoId { get; set; }
        public string? TipoUtilizacao { get; set; }
        public string? TipoUso { get; set; }
        public List<HospedesReservaAgendamentoModel>? Hospedes { get; set; }


        public static explicit operator CriacaoReservaAgendamentoModel(CriacaoReservaAgendamentoInputModel model)
        {
            var result = new CriacaoReservaAgendamentoModel
            {
                Hospedes = model.Hospedes,
                AgendamentoId = model.AgendamentoId,
                QuantidadeAdultos = model.QuantidadeAdultos,
                QuantidadeCrianca2 = model.QuantidadeCrianca2,
                QuantidadeCrianca1 = model.QuantidadeCrianca1,
                DataHora = DateTime.Now,
                DataHoraAConfirmar = model.Status == "AC" ? model.CheckIn.AddDays(-1) : DateTime.Today.AddDays(-1),
                DataHoraConfirmacao = model.Status == "CF" ? model.CheckIn.AddDays(-1) : null,
                DataHoraCancelamento = model.Status == "CL" ? DateTime.Now : null,
                CheckOut = model.CheckOut,
                CheckIn = model.CheckIn,
                TipoPensao = Enums.EnumReservaTipoPensao.NN,
                Status = Enums.EnumReservaStatus.AC,
                TipoTarifacao = Enums.EnumTipoTarifacao.DiaDia,
                Id = model.Id
            };

            return result;
        }
    }


}
