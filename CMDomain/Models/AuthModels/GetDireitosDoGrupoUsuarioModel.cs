namespace CMDomain.Models.AuthModels
{
    public class GetDireitoGrupoAcessoModel : ModelRequestBase
    {
        public int? GrupoId { get; set; }
        public int? EmpresaId { get; set; }
        public int? ModuloId { get; set; }
        public string? ModuloNome { get; set; }

    }
}
