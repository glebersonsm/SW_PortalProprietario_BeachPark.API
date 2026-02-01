namespace CMDomain.Models.AuthModels
{
    public class RemoverDireitoDoGrupoUsuarioInputModel : ModelRequestBase
    {
        public int? GrupoId { get; set; }
        public List<int> EmpresasIds { get; set; } = new List<int>();
        public List<int> IdOperFuncIdsRemover { get; set; } = new List<int>();

    }
}
