namespace AccessCenterDomain.AccessCenter
{
    public class UsuarioFilial : EntityBase
    {
        public virtual int? Usuario { get; set; }
        public virtual int? Filial { get; set; }

    }
}
