namespace SW_PortalProprietario.Application.Models.FrameworkModels
{
    public class EmpresaSimplificadaModel : ModelBase
    {
        public EmpresaSimplificadaModel()
        {

        }

        public string? Codigo { get; set; }
        public string? Nome { get; set; }
        public string? NomeFantasia { get; set; }
        public string? Email { get; set; }
        public string? Cnpj { get; set; }

    }
}
