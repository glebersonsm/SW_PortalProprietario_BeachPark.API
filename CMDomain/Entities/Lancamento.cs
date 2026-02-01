namespace CMDomain.Entities
{
    public class Lancamento : CMEntityBase
    {
        public virtual int? PlnCodigo { get; set; }
        public virtual int? LacNumLan { get; set; }
        public virtual string? LacDebCre { get; set; }
        public virtual int? UnidNegoc { get; set; }
        public virtual int? IdPlanoPrev { get; set; }
        public virtual int? IdPatro { get; set; }
        public virtual int? IdElemDemonstrat { get; set; }
        public virtual string? HitCodHist { get; set; }
        public virtual int? IdPessoa { get; set; }
        public virtual int? IdEmpresa { get; set; }
        public virtual int? IdModulo { get; set; }
        public virtual int? IdUsuarioInclusao { get; set; }
        public virtual string? CodCentroCusto { get; set; }
        public virtual string? Placonta { get; set; }
        public virtual int? Plano { get; set; }
        public virtual string? LacTipo { get; set; }
        public virtual string? LacNumDoc { get; set; }
        public virtual string? LacHist1 { get; set; }
        public virtual string? LacHist2 { get; set; }
        public virtual string? LacHist3 { get; set; }
        public virtual string? LacHist4 { get; set; }
        public virtual string? LacHist5 { get; set; }
        public virtual decimal? LacValor { get; set; }
        public virtual string? LacTipConvOficial { get; set; } = "N";
        public virtual decimal? LacValOficial { get; set; }
        public virtual string? LacTipConvGer { get; set; } = "N";
        public virtual decimal? LacValGerencial { get; set; } = 0.00m;
        public virtual string? LacTipConvGeren1 { get; set; } = "N";
        public virtual decimal? LacValGeren1 { get; set; } = 0.00m;
        public virtual string? LacTipConvGeren2 { get; set; } = "N";
        public virtual decimal? LacValGeren2 { get; set; } = 0.00m;
        public virtual string? LacatOutMoeda { get; set; } = "N";
        public virtual string? LacOrigemAplic { get; set; }
        public virtual string? TipCodigo { get; set; }
        public virtual decimal? LacValHist { get; set; }
        public virtual int? CodSubConta { get; set; }
        public virtual int? LoteTransmissao { get; set; }
        public virtual int? Num_Reserva_Orig { get; set; }
        public virtual string? LacTipConvMoeHis { get; set; }
        public virtual string? PrCodigoItem { get; set; }
        public virtual int? PrQtdInicial { get; set; }
        public virtual string? PrIdentIndividual { get; set; }
        public virtual string? PrTipo { get; set; }
        public virtual string? PrDescricaoItem { get; set; }
        public virtual DateTime? PrDataReconhec { get; set; }
        public virtual string? PrCnpjEmpInvest { get; set; }
        public virtual decimal? PrParcelaRealiz { get; set; } = 0.00m;
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }

        public override int GetHashCode()
        {
            return PlnCodigo.GetHashCode() + LacNumLan.GetHashCode() + LacDebCre.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            Lancamento? lancamento = obj as Lancamento;
            if (lancamento is null) return false;
            return lancamento.Equals(this);
        }

    }
}
