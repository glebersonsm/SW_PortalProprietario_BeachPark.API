namespace SW_PortalProprietario.Application.Models.GeralModels
{
    public class EmailInputInternalModel
    {
        public int UsuarioCriacao { get; set; }
        public int? EmpresaId { get; set; }
        public string? Assunto { get; set; }
        public string? Destinatario { get; set; }
        public string? ConteudoEmail { get; set; }
        public List<EmailAnexoInputModel>? Anexos { get; set; }
    }

}
