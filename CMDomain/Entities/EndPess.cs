namespace CMDomain.Entities
{
    public class EndPess : CMEntityBase
    {
        public virtual int? IdEndereco { get; set; }
        public virtual int? IdPessoa { get; set; }
        public virtual int? IdCidades { get; set; }
        public virtual string? Logradouro { get; set; }
        public virtual string? Numero { get; set; }
        public virtual string? Complemento { get; set; }
        public virtual string? Bairro { get; set; }
        public virtual string? Cep { get; set; }
        public virtual string? TipoEndereco { get; set; }
        public virtual string? Nome { get; set; }
        public virtual string? FlgTipoEnd { get; set; } = "U";
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }
        public virtual DateTime? TrgDtAlteracao { get; set; }
        public virtual string? TrgUserAlteracao { get; set; }
        public virtual string? CodigoIbge { get; set; }
        public virtual string? CidadeNome { get; set; }
        public virtual string? CidadeUf { get; set; }
        public virtual string? EstadoSigla { get; set; }

    }
}
