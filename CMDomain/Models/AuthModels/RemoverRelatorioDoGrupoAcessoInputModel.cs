namespace CMDomain.Models.AuthModels
{
    public class RemoverRelatorioDoGrupoAcessoInputModel : ModelRequestBase
    {
        public int? GrupoId { get; set; }
        public List<int> EmpresasIds { get; set; } = new List<int>();
        public List<int> RelatoriosIds { get; set; } = new List<int>();

    }
}
