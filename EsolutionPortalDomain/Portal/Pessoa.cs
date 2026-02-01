namespace EsolutionPortalDomain.Portal
{
    public class Pessoa : EntityBasePortal
    {
        public virtual string? Tipo { get; set; } = "F";
        public virtual DateTime? DataHoraCadastro { get; set; }
        public virtual int? UsuarioCadastro { get; set; }
        public virtual DateTime? DataHoraModificacao { get; set; }
        public virtual int? UsuarioModificacao { get; set; }
        public virtual string? Nome { get; set; }
        public virtual string? NomeFantasia { get; set; }
        public virtual int? EstadoCivil { get; set; }
        public virtual int? Sexo { get; set; }
        public virtual DateTime? Nascimento { get; set; }
        public virtual string? RG { get; set; }
        public virtual string? CPF { get; set; }
        public virtual string? eMail { get; set; }
        public virtual decimal? Renda { get; set; }
        public virtual string? Estrangeiro { get; set; } = "N";
        public virtual int? IntegracaoStatus { get; set; } = 1;
        public virtual int? IntegracaoTotalTentativas { get; set; } = 0;

    }
}
