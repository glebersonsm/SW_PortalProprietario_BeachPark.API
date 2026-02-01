namespace CMDomain.Models.AuthModels
{
    public class RemoverRelatorioDoUsuarioInputModel : ModelRequestBase
    {
        public int? UsuarioId { get; set; }
        public List<int> EmpresasIds { get; set; } = new List<int>();
        public List<int> RelatoriosIdsRemover { get; set; } = new List<int>();

    }
}
