namespace CMDomain.Models.AuthModels
{
    public class AdicionarDireitoAoUsuarioInputModel : ModelRequestBase
    {
        public int? UsuarioId { get; set; }
        public List<int> EmpresasIds { get; set; } = new List<int>();
        public List<DireitoAdicionarInputModel> OperFuncAdicionar { get; set; } = new List<DireitoAdicionarInputModel>();

    }
}
