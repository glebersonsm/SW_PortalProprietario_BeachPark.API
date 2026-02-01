namespace SW_PortalProprietario.Application.Models.SystemModels
{
    public class UsuarioModuloPermissaoModel : ModelBase
    {
        public int? PermissaoId { get; set; }
        public string? PermissaoNome { get; set; }
        public string? PermissaoNomeInterno { get; set; }
        public string? TipoPermissao { get; set; }
        public int? ModuloId { get; set; }
        public string? ModuloCodigo { get; set; }
        public string? ModuloNome { get; set; }

    }
}
