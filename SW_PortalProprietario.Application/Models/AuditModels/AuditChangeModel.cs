namespace SW_PortalProprietario.Application.Models.AuditModels
{
    public class AuditChangeModel
    {
        public string PropertyName { get; set; } = string.Empty;
        public object? OldValue { get; set; }
        public object? NewValue { get; set; }
        public string? FriendlyMessage { get; set; }
    }
}

