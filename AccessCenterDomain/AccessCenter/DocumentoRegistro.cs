namespace AccessCenterDomain.AccessCenter
{
    public class DocumentoRegistro : EntityBase
    {
        public virtual int? Pessoa { get; set; }
        public virtual int? TipoDocumentoRegistro { get; set; }
        public virtual string? DocumentoAlfanumerico { get; set; }
        public virtual Int64? DocumentoNumerico { get; set; }
        public virtual string Principal { get; set; } = "N";
        public virtual string Tipo { get; set; } = "A";

    }
}
