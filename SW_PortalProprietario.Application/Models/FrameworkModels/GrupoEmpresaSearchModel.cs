using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Models.FrameworkModels
{
    public class GrupoEmpresaSearchModel
    {
        public int? Id { get; set; }
        public int? PessoaId { get; set; }
        public string? Codigo { get; set; }
        public string? Nome { get; set; }
        public EnumStatus? Status { get; set; }
        public bool? PopularEmpresa { get; set; }
        public bool? CarregarPessoaCompleta { get; set; }

    }
}
