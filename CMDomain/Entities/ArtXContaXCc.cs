namespace CMDomain.Entities
{
    public class ArtXContaXCc : CMEntityBase
    {
        public virtual int? IdArtXContaXCc { get; set; }
        public virtual int? CodAlmoxarifado { get; set; }
        public virtual int? IdPessoa { get; set; }
        public virtual int? Plano { get; set; }
        public virtual int? IdEmpresa { get; set; }
        public virtual string? CodCentroCusto { get; set; }
        public virtual int? UnidNegoc { get; set; }
        public virtual string? ContaEntrada { get; set; }
        public virtual int? SubContaEntrada { get; set; }
        public virtual string? ContaSaida { get; set; }
        public virtual int? SubContaSaida { get; set; }
        public virtual string? CodArtigo { get; set; }
        public virtual string? CodGrupoProd { get; set; }
        public virtual string? TrgUserInclusao { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }

    }
}
