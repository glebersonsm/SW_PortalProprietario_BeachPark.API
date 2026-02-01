namespace CMDomain.Models.AuthModels
{
    public class RemoverDireitoDoUsuarioInputModel : ModelRequestBase
    {
        public int? UsuarioId { get; set; }
        public List<int> EmpresasIds { get; set; } = new List<int>();
        public List<int> IdOperFuncIdsRemover { get; set; } = new List<int>();

    }
}
