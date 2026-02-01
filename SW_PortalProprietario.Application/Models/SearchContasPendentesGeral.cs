namespace SW_PortalProprietario.Application.Models
{
    public class SearchContasPendentesGeral
    {
        public DateTime? VencimentoInicial { get; set; }
        public DateTime? VencimentoFinal { get; set; }
        public string? PessoaNome { get; set; }
        public int? EmpresaId { get; set; }
        public int? NumeroDaPagina { get; set; }
        public int? QuantidadeRegistrosRetornar { get; set; }
        public string? Status { get; set; } //T = Todas, P = Pendente, B = Baixada, V = Vencidas

    }
}
