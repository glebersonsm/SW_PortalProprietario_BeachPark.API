using EsolutionPortalDomain.Portal;

namespace SW_PortalProprietario.Application.Models.Empreendimento
{
    public class TrocaSemanaInputModel : LiberacaoMeuAgendamentoInputModel
    {
        public int? SemanaId { get; set; }
        public string? IdIntercambiadora { get; set; }
        public string? TipoUso { get; set; }
        public string? TipoUtilizacao { get; set; }
        public bool? TrocaDeTipoDeUso { get; set; } = false;

    }
}
