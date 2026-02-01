namespace CMDomain.Entities
{
    public class Conver : CMEntityBase
    {
        public virtual string? CodProduto { get; set; }
        public virtual string? CodMedida { get; set; }
        public virtual decimal? Fator { get; set; }
        public virtual string? FlgAtivo { get; set; }
        public virtual string? TrgUserInclusao { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }
        public override int GetHashCode()
        {
            return CodProduto.GetHashCode() + CodMedida.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            Conver? cc = obj as Conver;
            if (cc is null) return false;
            return cc.Equals(this);
        }
    }
}
