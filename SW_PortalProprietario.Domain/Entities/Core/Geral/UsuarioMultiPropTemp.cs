namespace SW_PortalProprietario.Domain.Entities.Core.Geral
{
    public class UsuarioMultiPropTemp : EntityBaseCore, IEntityValidateCore
    {
        public virtual string? Nome { get; set; }
        public virtual string? CpfCnpj { get; set; }
        public virtual string? Cliente { get; set; }
        public virtual string? Email { get; set; }
        public virtual int? IdContrato { get; set; }
        public virtual string? NumeroContrato { get; set; }
        public virtual string? Administrador { get; set; } = "N";

        public virtual async Task SaveValidate()
        {
            await Task.CompletedTask;
        }

        public virtual object Clone()
        {
            return MemberwiseClone();
        }
    }
}
