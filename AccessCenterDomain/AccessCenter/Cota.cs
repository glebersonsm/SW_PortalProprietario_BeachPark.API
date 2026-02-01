namespace AccessCenterDomain.AccessCenter
{
    public class Cota : EntityBase
    {
        public virtual int? GrupoCotaTipoCota { get; set; }
        public virtual int? Imovel { get; set; }
        public virtual int? Proprietario { get; set; }
        public virtual int? Procurador { get; set; }
        public virtual string Status { get; set; } = "D";
        public virtual int? FrAtendimentoVenda { get; set; }
        public virtual int? CategoriaCota { get; set; }
        public virtual DateTime? DataAquisicao { get; set; }
        public virtual string Bloqueado { get; set; } = "N";
        public virtual string? ObservacaoBloqueio { get; set; }
        public virtual string? Observacao { get; set; }
        public virtual decimal? Percentual { get; set; }
        public virtual string PagaCondominio { get; set; } = "S";
        public virtual int? SequenciaVenda { get; set; } = 0;
        public virtual string? PadraoDeCor { get; set; } = "Default";
    }
}
