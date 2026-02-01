namespace CMDomain.Entities
{
    public class NfRecebDevol : CMEntityBase
    {
        public virtual int? IdNfRecebDevol { get; set; }
        public virtual int? IdHotel { get; set; }
        public virtual int? IdForCli { get; set; }
        public virtual int? CodDocumento { get; set; }
        public virtual string? NumNf { get; set; }
        public virtual string? IdRequestia { get; set; }
        public virtual int? Processado { get; set; }
        public virtual int? IdArquivo { get; set; }

    }
}
