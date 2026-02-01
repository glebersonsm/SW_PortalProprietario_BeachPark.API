namespace EsolutionPortalDomain.ReservasApiModels
{
    public class EmpresaModel
    {
        public int? EmpresaId { get; set; }
        public int? PessoaId { get; set; }
        public string? Nome { get; set; }
        public string? NomeFantasia { get; set; }
        public string? Cnpj { get; set; }
    }
}
