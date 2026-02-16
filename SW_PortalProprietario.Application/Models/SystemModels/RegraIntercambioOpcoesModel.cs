namespace SW_PortalProprietario.Application.Models.SystemModels
{
    /// <summary>
    /// Opções para configuração de regras de intercâmbio.
    /// Tipos de semana: eSolution (Média, Alta, Super Alta) e CM (Super alta, Alta, Média, Baixa).
    /// </summary>
    public class RegraIntercambioOpcoesModel
    {
        public List<TipoSemanaModel> TiposSemanaESolution { get; set; } = new();
        public List<TipoSemanaModel> TiposSemanaCM { get; set; } = new();
        public List<RegraIntercambioOpcaoItem> TiposContrato { get; set; } = new();
        public List<TipoUhEsolModel> TiposUhEsol { get; set; } = new();
    }

    public class RegraIntercambioOpcaoItem
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public int? Id { get; set; }
    }
}
