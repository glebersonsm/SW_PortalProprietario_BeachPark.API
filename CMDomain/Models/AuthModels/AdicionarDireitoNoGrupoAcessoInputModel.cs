namespace CMDomain.Models.AuthModels
{
    public class AdicionarDireitoNoGrupoAcessoInputModel : ModelRequestBase
    {
        public int? GrupoId { get; set; }
        public List<int> EmpresasIds { get; set; } = new List<int>();
        public List<DireitoAdicionarInputModel> OperFuncAdicionar { get; set; } = new List<DireitoAdicionarInputModel>();

    }
}
