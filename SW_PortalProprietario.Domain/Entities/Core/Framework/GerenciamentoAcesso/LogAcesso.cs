namespace SW_PortalProprietario.Domain.Entities.Core.Framework.GerenciamentoAcesso
{
    public class LogAcesso : EntityBaseCore, IEntityValidateCore
    {
        public virtual string? Guid { get; set; }
        public virtual DateTime? DataInicio { get; set; }
        public virtual DateTime? DataFinal { get; set; }
        public virtual string? UrlRequested { get; set; }
        public virtual string? ClientIpAddress { get; set; }
        public virtual string? RequestBody { get; set; }
        public virtual string? Response { get; set; }
        public virtual int? StatusResult { get; set; }

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
