namespace AccessCenterDomain.AccessCenter
{
    public class Pessoa : EntityBase
    {
        public virtual string? Tipo { get; set; } = "F";
        public virtual string? Nome { get; set; }
        public virtual string? NomePesquisa { get; set; }
        public virtual string? NomeFantasia { get; set; }
        public virtual string? NomeFantasiaPesquisa { get; set; }
        public virtual string? NomeExibicao { get; set; }
        public virtual Int64? Cnpj { get; set; }
        public virtual string? InscricaoEstadual { get; set; }
        public virtual string? InscricaoMunicipal { get; set; }
        public virtual string? InscricaoSubstituto { get; set; }
        public virtual string? Suframa { get; set; }
        public virtual string? Filial { get; set; } = "N";
        public virtual int? EstadoCivil { get; set; }
        public virtual int? RegimeCasamento { get; set; }
        public virtual Int64? CPF { get; set; }
        public virtual string? RG { get; set; }
        public virtual string? RGOrgaoExpedidor { get; set; }
        public virtual int? RGEstado { get; set; }
        public virtual string? Sexo { get; set; }
        public virtual DateTime? Nascimento { get; set; }
        public virtual string Estrangeiro { get; set; } = "N";
        public virtual string? Nacionalidade { get; set; }
        public virtual string Visivel { get; set; } = "S";
        public virtual string? eMail { get; set; }
        public virtual Int64? PessoaEnderecoPreferencial { get; set; }
        public virtual Int64? PessoaEnderecoCobranca { get; set; }
        public virtual int? PessoaTelefonePreferencial { get; set; }
        public virtual int? PessoaProfissaoPrincipal { get; set; }
        public virtual string? ConsumidorFinal { get; set; } = "S";
        public virtual int? Segmento { get; set; } = 1;
        public virtual int? RegimeTributacao { get; set; } = 1;
        public virtual string RecebeSMS { get; set; } = "N";
        public virtual string? Passaporte { get; set; }

    }
}
