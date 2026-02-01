namespace CMDomain.Models.AuthModels
{
    public class GrupoUsuarioDireitoEmpresaModel
    {
        public int? GrupoId { get; set; }
        public string? GrupoNome { get; set; }
        public string? Descricao { get; set; }
        public int? EmpresaId { get; set; }
        public string? EmpresaNome { get; set; }

        public string? ModuloId { get; set; }
        public string? ModuloNome { get; set; }
        public int? OperFuncId { get; set; }
        public int? OperacaoId { get; set; }
        public string? OperacaoNome { get; set; }
        public string? FuncaoId { get; set; }
        public string? FuncaoNome { get; set; }
        public bool? Visualizar { get; set; }
        public bool? Habilitar { get; set; }
    }
}
