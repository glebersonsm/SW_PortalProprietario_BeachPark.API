namespace CMDomain.Entities
{
    public class Almox : CMEntityBase
    {
        public virtual int? CodAlmoxarifado { get; set; }
        public virtual int? CodCusteio { get; set; }
        public virtual int? IdPessoa { get; set; }
        public virtual string? CodCentroCusto { get; set; }
        public virtual int? IdEmpresa { get; set; }
        public virtual string? DescAlmox { get; set; }
        public virtual string PrincipSecund { get; set; } = "P";
        public virtual string Contabil { get; set; } = "T";
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }
    }
}
