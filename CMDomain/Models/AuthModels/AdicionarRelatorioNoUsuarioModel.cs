namespace CMDomain.Models.AuthModels
{
    public class AdicionarRelatorioNoUsuarioInputModel : ModelRequestBase
    {
        public int? UsuarioId { get; set; }
        public List<int> EmpresasIds { get; set; } = new List<int>();
        public List<int> RelatoriosIds { get; set; } = new List<int>();

    }
}
