namespace CMDomain.Entities
{
    public class Funcao : CMEntityBase
    {
        public virtual int? IdFuncao { get; set; }
        public virtual string? NomeFuncao { get; set; }
        public virtual int? IdModulo { get; set; }
        public virtual int? IdFuncaoPai { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }

    }
}
