namespace CMDomain.Models.AuthModels
{
    public class InclusaoGrupoAcessoInputModel : ModelRequestBase
    {
        public string? GrupoNome { get; set; }
        public string? Descricao { get; set; }
        public int? DiasForcarAlteracaoSenha { get; set; }
        public List<int> EmpresasIds { get; set; } = new List<int>();
        public List<DireitoAdicionarInputModel> OperFuncAdicionar { get; set; } = new List<DireitoAdicionarInputModel>();
        public List<int> RelatoriosIds { get; set; } = new List<int>();
        public List<int> UsuariosIdsAdicionar { get; set; } = new List<int>();


    }
}
