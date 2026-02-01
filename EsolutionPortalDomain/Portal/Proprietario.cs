namespace EsolutionPortalDomain.Portal
{
    public class Proprietario : EntityBasePortal
    {
        public virtual int? Cliente { get; set; }
        public virtual int? CotaProprietario { get; set; }
        public virtual DateTime? DataHoraInclusao { get; set; }
        public virtual DateTime? DataHoraExclusao { get; set; }
        public virtual string? Principal { get; set; }
        public virtual int? UsuarioExclusao { get; set; }
        public virtual DateTime? DataTransferencia { get; set; }
        public virtual int? NovoProprietario { get; set; }
        public virtual string? NumeroContrato { get; set; }
        public virtual string? NomeProduto { get; set; }

    }
}
