namespace SW_PortalProprietario.Application.Models
{
    public class PosicaoAgendamentoViewModel
    {
        public int? QtdeSemanasDireitoUso { get; set; }
        public int? QtdeReservas { get; set; }
        public int? UhCondominio { get; set; }
        public string? UhCondominioNumero { get; set; }
        public int? CotaId { get; set; }
        public string? CotaNome { get; set; }
        public int? PrioridadeAgendamento { get; set; }
        public int? PessoaClienteId { get; set; }
        public string? PessoaClienteNome { get; set; }
        public DateTime? DataInicialAgendamento { get; set; }
        public DateTime? DataFinalAgendamento { get; set; }
        public int? TipoContrato { get; set; }

    }
}
