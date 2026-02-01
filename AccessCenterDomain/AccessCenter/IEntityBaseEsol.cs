namespace AccessCenterDomain.AccessCenter
{
    public interface IEntityBaseEsol
    {
        int Id { get; set; }
        int? UsuarioCriacao { get; set; }
        DateTime? DataHoraCriacao { get; set; }
        int? UsuarioAlteracao { get; set; }
        DateTime? DataHoraAlteracao { get; set; }
    }
}
