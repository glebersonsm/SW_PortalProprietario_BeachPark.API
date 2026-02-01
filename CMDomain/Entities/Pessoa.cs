namespace CMDomain.Entities
{
    public class Pessoa : CMEntityBase
    {
        public virtual int? IdPessoa { get; set; }
        public virtual int? IdDocumento { get; set; }
        public virtual string? Nome { get; set; }
        public virtual string? Tipo { get; set; } = "F";
        public virtual string? RazaoSocial { get; set; }
        public virtual int? FlgUsuario { get; set; } = 0;
        public virtual int? FlgCliente { get; set; } = 0;
        public virtual int? FlgOutro { get; set; } = 0;
        public virtual int? FlgTerceiro { get; set; } = 0;
        public virtual int? FlgFornServ { get; set; } = 0;
        public virtual int? FlgFuncionario { get; set; } = 0;
        public virtual int? FlgEstrangeiro { get; set; } = 0;
        public virtual int? FlgProdutor { get; set; } = 0;
        public virtual int? FlgAgencia { get; set; } = 0;
        public virtual int? FlgBanco { get; set; } = 0;
        public virtual string? NumDocumento { get; set; }
        public virtual string? Email { get; set; }
        public virtual int? IdEndCorresp { get; set; }
        public virtual int? IdEndComercial { get; set; }
        public virtual int? IdEndEntrega { get; set; }
        public virtual int? IdEndResidencial { get; set; }
        public virtual int? IdEndCobranca { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }
    }
}
