namespace CMDomain.Entities
{
    public class CentRespon : CMEntityBase
    {
        public virtual string? CodCentroRespon { get; set; } = "";
        public virtual int IdPessoa { get; set; }
        public virtual string? Nome { get; set; }
        public virtual string? AnaliticoSintet { get; set; }
        public virtual string? Ativo { get; set; } = "S";
        public override int GetHashCode()
        {
            return Convert.ToString(CodCentroRespon).GetHashCode() + IdPessoa.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            CentRespon? cc = obj as CentRespon;
            if (cc is null) return false;
            return cc.Equals(this);
        }
    }
}
