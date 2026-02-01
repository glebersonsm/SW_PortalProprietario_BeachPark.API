namespace SW_PortalProprietario.Application.Models
{
    public class ModelBase
    {
        public int? Id { get; set; }
        public DateTime? DataHoraCriacao { get; set; }
        public int? UsuarioCriacao { get; set; }
        public string? NomeUsuarioCriacao { get; set; }
        public DateTime? DataHoraAlteracao { get; set; }
        public int? UsuarioAlteracao { get; set; }
        public string? NomeUsuarioAlteracao { get; set; }
    }
}
