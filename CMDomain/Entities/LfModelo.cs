namespace CMDomain.Entities
{
    public class LfModelo : CMEntityBase
    {
        public virtual int? IdModelo { get; set; }
        public virtual string? Modelo { get; set; }
        public virtual string? Descricao { get; set; }
        public virtual string? Sigla { get; set; }
        public virtual string? ModeloSped { get; set; }
        public virtual string? FlgConsideraDtSaida { get; set; }
        public virtual string? FlgTipo { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }

    }
}
