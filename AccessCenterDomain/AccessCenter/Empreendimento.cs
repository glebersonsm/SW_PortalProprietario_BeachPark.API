namespace AccessCenterDomain.AccessCenter
{
    public class Empreendimento : EntityBase
    {
        public virtual int? Filial { get; set; }
        public virtual int? GrupoEmpresa { get; set; }
        public virtual int? Empresa { get; set; }
        public virtual string Codigo { get; set; }
        public virtual string Nome { get; set; }
        public virtual string NomePesquisa { get; set; }
        public virtual string AlterarClienteFinanceiro { get; set; } = "N";
        public virtual string TipoControlePeriodo { get; set; } = "F";
        public virtual string AlterarCategoriaCotaFin { get; set; } = "F";
        public virtual string Entregue { get; set; } = "N";
        public virtual int? MesPrazoEntrega { get; set; }
        public virtual int? AnoPrazoEntrega { get; set; }
        public virtual decimal? ValorEstimadoCondominio { get; set; }
        public virtual int? MesPrazoInicioCondominio { get; set; }
        public virtual int? AnoPrazoInicioCondominio { get; set; }
        public virtual decimal? TaxaPercentualUtilizacao { get; set; }
        public virtual string Logradouro { get; set; }
        public virtual string Numero { get; set; }
        public virtual string Bairro { get; set; }
        public virtual string Cep { get; set; }
        public virtual string Complemento { get; set; }
        public virtual int? Cidade { get; set; }
        public virtual DateTime? DataUltimoRefracionamento { get; set; }
        public virtual string AlterarClienteFinanceiroCan { get; set; } = "N";

    }
}
