namespace SW_PortalProprietario.Application.Models.FrameworkModels
{
    public class EmpresaSearchModel
    {
        public int? Id { get; set; }
        public int? PessoaId { get; set; }
        public string? Codigo { get; set; }
        public string? Nome { get; set; }
        public bool? CarregarPessoaCompleta { get; set; }

    }
}
