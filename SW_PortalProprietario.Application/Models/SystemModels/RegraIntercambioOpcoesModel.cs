using SW_PortalProprietario.Application.Models.TimeSharing;

namespace SW_PortalProprietario.Application.Models.SystemModels
{
    /// <summary>
    /// OpÃ§Ãµes para configuraÃ§Ã£o de regras de intercÃ¢mbio.
    /// Tipos de semana: eSolution (MÃ©dia, Alta, Super Alta) e CM (Super alta, Alta, MÃ©dia, Baixa).
    /// </summary>
    public class RegraIntercambioOpcoesModel
    {
        public List<TipoSemanaModel> TiposSemanaESolution { get; set; } = new();
        public List<TipoSemanaModel> TiposSemanaCM { get; set; } = new();
        public List<RegraIntercambioOpcaoItem> TiposContrato { get; set; } = new();
        public List<TipoUhEsolModel> TiposUhEsol { get; set; } = new();
        public List<TipoUhCmModel> TiposUhCM { get; set; } = new();
    }

    public class RegraIntercambioOpcaoItem
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public int? Id { get; set; }
    }
}
