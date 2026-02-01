namespace SW_PortalProprietario.Application.Models.SystemModels
{
    public class UserRoleModel : ModelBase
    {
        public string? ModuleInternalName { get; set; }
        public string? PermissionType { get; set; }
        public string? NormalizedPermission => $"{ModuleInternalName}={PermissionType}";

    }
}
