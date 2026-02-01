namespace CMDomain.Entities
{
    public class TelEndPess : CMEntityBase
    {
        public virtual int? IdTelefone { get; set; }
        public virtual int? IdEndereco { get; set; }
        public virtual string? Ddi { get; set; }
        public virtual string? Ddd { get; set; }
        public virtual string? Tipo { get; set; }
        public virtual string? Numero { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }
        public virtual DateTime? TrgDtAlteracao { get; set; }
        public virtual string? TrgUserAlteracao { get; set; }

    }
}
