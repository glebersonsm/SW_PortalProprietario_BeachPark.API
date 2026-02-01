namespace CMDomain.Models
{
    public class ModelRequestEmpresaCodigoString : ModelRequestBase
    {
        public int? EmpresaId { get; set; }
        public string? Codigo { get; set; }
    }
}
