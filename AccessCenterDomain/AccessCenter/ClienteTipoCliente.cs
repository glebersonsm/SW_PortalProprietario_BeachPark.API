namespace AccessCenterDomain.AccessCenter
{
    public class ClienteTipoCliente : EntityBase
    {
        public virtual int? TipoCliente { get; set; }
        public virtual int? Cliente { get; set; }
        public virtual DateTime? DataHoraRemocao { get; set; }

    }
}
