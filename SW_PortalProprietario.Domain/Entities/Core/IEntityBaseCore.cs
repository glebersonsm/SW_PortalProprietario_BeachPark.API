namespace SW_PortalProprietario.Domain.Entities.Core
{
    public interface IEntityBaseCore
    {
        int Id { get; set; }
        int? UsuarioCriacao { get; set; }
        DateTime? DataHoraCriacao { get; set; }
        int? UsuarioAlteracao { get; set; }
        DateTime? DataHoraAlteracao { get; set; }
        string? ObjectGuid { get; set; }
    }
}
