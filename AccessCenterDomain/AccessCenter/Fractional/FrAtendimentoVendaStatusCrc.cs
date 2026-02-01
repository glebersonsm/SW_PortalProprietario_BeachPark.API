namespace AccessCenterDomain.AccessCenter.Fractional
{
    public class FrAtendimentoVendaStatusCrc : EntityBase
    {
        public virtual string IntegracaoId { get; set; }
        public virtual string Status { get; set; } = "A";
        public virtual DateTime? DataHoraInativacao { get; set; }
        public virtual int? UsuarioInativacao { get; set; }
        public virtual int? FrAtendimentoVenda { get; set; }
        public virtual string Observacao { get; set; }
        public virtual int? FrStatusCrc { get; set; }


    }
}
