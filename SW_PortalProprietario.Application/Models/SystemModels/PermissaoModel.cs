namespace SW_PortalProprietario.Application.Models.SystemModels
{
    public class PermissaoModel : ModelBase
    {
        public int? UserId { get; set; }
        public string? NomeModulo { get; set; }
        public string? TipoPermissao { get; set; }

    }
}
