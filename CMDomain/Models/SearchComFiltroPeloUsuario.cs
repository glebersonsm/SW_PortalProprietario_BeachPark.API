namespace CMDomain.Models
{
    public class SearchComFiltroPeloUsuario : ModelRequestBase
    {
        public string? CodCentroCusto { get; set; }
        public int? EmpresaId { get; set; }
        public string? Nome { get; set; }
        public bool? ListarApenasVinculadoAoUsuario { get; set; } = false;
        public bool? ApenasAtivos { get; set; } = true;

    }
}
