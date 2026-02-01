namespace AccessCenterDomain.AccessCenter
{
    public class Filial : EntityBase
    {
        public virtual int? Empresa { get; set; }
        public virtual int? Pessoa { get; set; }
        public virtual string NomeAbreviado { get; set; }
        public virtual string Codigo { get; set; }
        public virtual string UtilizaIpi { get; set; }
        public virtual string InformaRegistroIpiZeradoSped { get; set; } = "N";
        public virtual string UtilizaSubstituicaoTributaria { get; set; } = "N";
        public virtual int? TipoFilial { get; set; }
        public virtual int? TipoComercio { get; set; }
        public virtual string Sigla { get; set; }
        public virtual string UtilizaEcf { get; set; } = "N";
        public virtual string Status { get; set; } = "A";

    }
}
