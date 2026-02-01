namespace SW_PortalProprietario.Application.Models.GeralModels
{
    public class AlteracaoEmailInputModel : CreateUpdateModelBase
    {
        public int? EmpresaId { get; set; }
        public string? Assunto { get; set; }
        public string? Destinatario { get; set; }
        public string? ConteudoEmail { get; set; }

    }

}
