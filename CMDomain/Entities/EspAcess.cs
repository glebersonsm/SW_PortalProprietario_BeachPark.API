namespace CMDomain.Entities
{
    public class EspAcess : CMEntityBase
    {
        public virtual int? IdEspAcesso { get; set; }
        public virtual int? IdImagem { get; set; }
        public virtual string? CorFundoMain { get; set; }
        public virtual string? FlgClassicViewer { get; set; } = "N";
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }

    }
}
