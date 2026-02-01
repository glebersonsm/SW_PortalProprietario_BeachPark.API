namespace CMDomain.Entities
{
    public class Saldo : CMEntityBase
    {
        public virtual string CodArtigo { get; set; }
        public virtual int CodAlmoxarifado { get; set; }
        public virtual decimal? SaldoQtde { get; set; }
        public virtual int? IdPessoa { get; set; }
        public virtual string? TrgUserInclusao { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }

        public override int GetHashCode()
        {
            return CodArtigo.GetHashCode() + CodAlmoxarifado.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            Saldo? cc = obj as Saldo;
            if (cc is null) return false;
            return cc.Equals(this);
        }
    }
}
