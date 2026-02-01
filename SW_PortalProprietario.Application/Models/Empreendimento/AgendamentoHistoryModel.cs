namespace SW_PortalProprietario.Application.Models.Empreendimento
{
    public class AgendamentoHistoryModel
    {
        public string? OperacaoId { get; set; }
        public string? LoginUsuario { get; set; }
        public string? NomeUsuario { get; set; }
        public int? AgendamentoId { get; set; }
        public string? TipoOperacao { get; set; }
        public DateTime? DataOperacao { get; set; }
        public DateTime? DataConfirmacao { get; set; }
        public string? Historico { get; set; }
        public string? Tentativas { get; set; }

    }
}
