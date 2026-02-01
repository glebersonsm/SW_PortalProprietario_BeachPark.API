namespace SW_PortalProprietario.Domain.Entities.Core.Framework.GerenciamentoAcesso
{
    public class LogAcessoObjetoCampo : EntityBaseCore, IEntityValidateCore
    {
        public virtual string? TipoCampo { get; set; }
        public virtual string? NomeCampo { get; set; }
        public virtual string? ValorAntes { get; set; }
        public virtual string? ValorApos { get; set; }

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
