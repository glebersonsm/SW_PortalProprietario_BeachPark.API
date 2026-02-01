namespace CMDomain.Entities
{
    public class CentCust : CMEntityBase
    {
        public virtual string CodCentroCusto { get; set; } = "";
        public virtual int IdEmpresa { get; set; }
        public virtual string? Nome { get; set; }
        public virtual string? StatusGrupoCDC { get; set; } = "A";
        public virtual string? Ativo { get; set; } = "S";
        public virtual string? CodExterno { get; set; }
        public virtual string? TrgUserInclusao { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }

        public override int GetHashCode()
        {
            return Convert.ToString(CodCentroCusto).GetHashCode() + IdEmpresa.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            CentCust? cc = obj as CentCust;
            if (cc is null) return false;
            return cc.Equals(this);
        }
    }
}
