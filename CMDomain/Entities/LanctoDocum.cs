namespace CMDomain.Entities
{
    public class LanctoDocum : CMEntityBase
    {
        public virtual int? CodDocumento { get; set; }
        public virtual int? NumLancto { get; set; }
        public virtual int? IdPessoa { get; set; }
        public virtual DateTime? DataLancto { get; set; }
        public virtual decimal? Valor { get; set; }
        public virtual string? DebCre { get; set; }
        public virtual decimal? ValorOutraMoeda { get; set; }
        public virtual string? Operacao { get; set; }
        public virtual int? IdUsuarioInclusao { get; set; }
        public virtual int? Estorno { get; set; }
        public virtual decimal? VlrLiquido { get; set; }
        public virtual string? NumRecibo { get; set; }
        public virtual int? CodTipDoc { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }
        public virtual int? CodAlterador { get; set; }
        public virtual string? HistoricoCompl { get; set; }
        public virtual int? PlnCodigo { get; set; }
        public virtual string? FlgContabilizado { get; set; }
        public virtual DateTime? TrgDtAlteracao { get; set; }
        public virtual string? TrgUserAlteracao { get; set; }

        public override int GetHashCode()
        {
            return CodDocumento.GetHashCode() + NumLancto.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            LanctoDocum? cc = obj as LanctoDocum;
            if (cc is null) return false;
            return cc.Equals(this);
        }

    }
}
