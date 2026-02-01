namespace AccessCenterDomain.AccessCenter
{
    public class ContaReceber : EntityBase
    {
        public virtual int? Filial { get; set; }
        public virtual int? Titulo { get; set; }
        public virtual string Documento { get; set; }
        public virtual string Observacao { get; set; }
        public virtual DateTime? Emissao { get; set; }
        public virtual int? Cliente { get; set; }
        public virtual int? ClienteAnterior { get; set; }
        public virtual decimal? Valor { get; set; }
        public virtual decimal? ValorOriginal { get; set; }
        public virtual int? PDV { get; set; } = 1;
        public virtual string Importacao { get; set; } = "S";
        public virtual int? OperacaoFinanceira { get; set; } = 1;
        public virtual DateTime? DataMovimento { get; set; }
        public virtual int? Operador { get; set; } = 1;
        public virtual int? QuantidadeParcelas { get; set; } = 1;
        public virtual int? GrupoEmpresa { get; set; } = 1;
        public virtual int? Empresa { get; set; } = 1;
        public virtual string IntegracaoId { get; set; }
        public virtual int? Cota { get; set; }

    }
}
