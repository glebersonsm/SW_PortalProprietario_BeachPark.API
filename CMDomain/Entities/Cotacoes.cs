namespace CMDomain.Entities
{
    public class Cotacoes : CMEntityBase
    {
        public virtual int? CodProcesso { get; set; }
        public virtual int? IdProcXArt { get; set; }
        public virtual int? IdForCli { get; set; }
        public virtual int? Proposta { get; set; }
        public virtual decimal? QtdeFornecida { get; set; }
        public virtual int? IdCondicoesPagto { get; set; }
        public virtual int? IdItemOc { get; set; }
        public virtual string? CodMedida { get; set; }
        public virtual int? NumCot { get; set; }
        public virtual DateTime? DataCot { get; set; }
        public virtual string? Status { get; set; }
        public virtual string? Obs { get; set; }
        public virtual decimal? TxJuros { get; set; }
        public virtual decimal? PrecoAValorPres { get; set; }
        public virtual string? Contato { get; set; }
        public virtual decimal? PrecoAValorEstoq { get; set; }
        public virtual int? IdArquivo { get; set; }
        public virtual decimal? Preco { get; set; }
        public virtual string? TrgUserInclusao { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }

        public override int GetHashCode()
        {
            return IdProcXArt.GetHashCode() + IdForCli.GetHashCode() + CodProcesso.GetHashCode() + Proposta.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            Cotacoes? cc = obj as Cotacoes;
            if (cc is null) return false;
            return cc.Equals(this);
        }
    }
}
