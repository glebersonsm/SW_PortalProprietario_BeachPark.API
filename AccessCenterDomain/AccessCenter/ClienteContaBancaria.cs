namespace AccessCenterDomain.AccessCenter
{
    public class ClienteContaBancaria : EntityBase
    {
        public virtual int? Cliente { get; set; }
        public virtual int? Banco { get; set; }
        public virtual string? Agencia { get; set; }
        public virtual string? AgenciaDigito { get; set; }
        public virtual string? Conta { get; set; }
        public virtual string? ContaDigito { get; set; }
        public virtual string? Variacao { get; set; }
        public virtual string? TipoConta { get; set; } = "C";
        public virtual string? Status { get; set; } = "A";
        public virtual string? InformarFavorecido { get; set; } = "N";
        public virtual string? Preferencial { get; set; } = "N";
        public virtual int? Cidade { get; set; }
        public virtual string? TipoChavePix { get; set; }
        public virtual string? ChavePix { get; set; }
        public virtual string? Tipo { get; set; } = "P";//P = Pix, B = Bancária
        public virtual string? InformaPix { get; set; } = "N";

    }
}
