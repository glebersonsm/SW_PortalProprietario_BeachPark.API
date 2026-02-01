namespace AccessCenterDomain.AccessCenter.Fractional
{
    public class FrLancamentoPontoOriLanFut : EntityBase
    {
        public virtual int? GrupoEmpresa { get; set; } = 1;
        public virtual int? Empresa { get; set; } = 1;
        public virtual int? Filial { get; set; } = 1;
        public virtual int? PontoUtilizado { get; set; }
        public virtual int? FrLancamentoPontoVinculado { get; set; }
        public virtual int? FrLancamentoPontoFuturo { get; set; }
        public virtual string LancamentoEstorno { get; set; } = "N";
    }
}
