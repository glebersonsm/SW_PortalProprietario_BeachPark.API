namespace CMDomain.Models.AuthModels
{
    public class UsuarioInputModel : ModelRequestBase
    {
        public string? NomeDoUsuario { get; set; }
        public string? NomeCompleto { get; set; }
        public string? Documento { get; set; }
        public int? IdTipoDocumento { get; set; }
        public int? PessoaId { get; set; }
        public string? Descricao { get; set; }
        public bool? MudarSenhaNoPrimeiroLogon { get; set; }
        public bool? UsuarioNaoPodeMudarSenha { get; set; }
        public bool? SenhaPermanente { get; set; }
        public List<int> EmpresasIds { get; set; } = new List<int>();
        public List<DireitoAdicionarInputModel> OperFuncAdicionar { get; set; } = new List<DireitoAdicionarInputModel>();
        public List<AlmoxarifadoPessoaInputModel> AlmoxarifadosAdicionar { get; set; } = new List<AlmoxarifadoPessoaInputModel>();
        public List<int> RelatoriosIds { get; set; } = new List<int>();
        public List<int> GruposIdsAdicionar { get; set; } = new List<int>();
    }
}
