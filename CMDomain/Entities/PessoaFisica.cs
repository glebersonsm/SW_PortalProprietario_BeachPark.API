namespace CMDomain.Entities
{
    public class PessoaFisica : CMEntityBase
    {
        public virtual int? IdPessoa { get; set; }
        public virtual string? Sexo { get; set; }
        public virtual DateTime? DataNasc { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }
    }
}
