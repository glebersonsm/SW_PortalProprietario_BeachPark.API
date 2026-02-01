namespace CMDomain.Entities
{
    public class Processo : CMEntityBase
    {
        public virtual int? CodProcesso { get; set; }

        //P = Preenchido, C = Cancelado, F = Finalizado, S = Solicitado
        public virtual string? Status { get; set; }
        public virtual int? IdComprador { get; set; }
        public virtual int? IdProcesso { get; set; }
        public virtual string? CodGrupoProd { get; set; }
        public virtual string? TrgUserInclusao { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }

    }
}
