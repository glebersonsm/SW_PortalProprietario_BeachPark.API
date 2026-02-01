namespace CMDomain.Entities
{
    public class LfItemXEmpresa : CMEntityBase
    {
        public virtual string? CodItem { get; set; }
        public virtual int? IdPessoa { get; set; }
        public virtual string? CodStIpi { get; set; }
        public virtual string? CodStCofins { get; set; }
        public virtual string? CodStPis { get; set; }
        public virtual string? CodStb { get; set; }
        public virtual string? FlgTipo { get; set; }
        public virtual string? FlgRegApuPisCof { get; set; }
        public virtual string? FlgRegra { get; set; }
        public virtual string? FlgIndicadorProp { get; set; }
        public virtual string? ContaContDebito { get; set; }
        public virtual string? ContaContCredito { get; set; }
        public virtual string? CentroCustoCredito { get; set; }
        public virtual string? CentroCustoDebito { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }
        public override int GetHashCode()
        {
            return CodItem.GetHashCode() + IdPessoa.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            LfItemXEmpresa? cc = obj as LfItemXEmpresa;
            if (cc is null) return false;
            return cc.Equals(this);
        }
    }
}
