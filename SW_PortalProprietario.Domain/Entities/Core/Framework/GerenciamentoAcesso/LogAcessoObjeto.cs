namespace SW_PortalProprietario.Domain.Entities.Core.Framework.GerenciamentoAcesso
{
    public class LogAcessoObjeto : EntityBaseCore, IEntityValidateCore
    {
        public virtual string? ObjectType { get; set; }
        public virtual string? ObjectOperationGuid { get; set; }
        public virtual int? ObjectId { get; set; }
        public virtual DateTime? DataHoraOperacao { get; set; }
        public virtual int? UsuarioOperacao { get; set; }
        public virtual string? TipoOperacao { get; set; }

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
