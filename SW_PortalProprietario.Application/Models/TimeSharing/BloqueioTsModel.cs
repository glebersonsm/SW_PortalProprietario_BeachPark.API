namespace SW_PortalProprietario.Application.Models.TimeSharing
{
    public class BloqueioTsModel
    {
        public string? FlgLiberado { get; set; }
        public string? Observacao {  get; set; }
        public string? Descricao { get; set; }
        public DateTime? DataBloqueio { get; set; }
        public string? UsuarioBloqueio { get; set; }
        public string? NumeroContrato { get; set; }
    }
}
