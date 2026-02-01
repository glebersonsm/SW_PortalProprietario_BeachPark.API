namespace CMDomain.Models.AuthModels
{
    public class RelatoriosLiberadosUsuarioModel
    {
        public int? UsuarioId { get; set; }
        public string? UsuarioNome { get; set; }
        public string? Descricao { get; set; }
        public int? EmpresaId { get; set; }
        public string? EmpresaNome { get; set; }
        public string? ModuloId { get; set; }
        public string? ModuloNome { get; set; }
        public int? GrupoRelatorioId { get; set; }
        public string? GrupoRelatorioNome { get; set; }

        public int? RelatorioId { get; set; }
        public string? RelatorioNome { get; set; }
        public bool? Visualizar { get; set; }
        public bool? Habilitar { get; set; }
        public bool? OrigemCm { get; set; }
        public string? DireitoOrigem { get; set; }

    }
}
