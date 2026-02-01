namespace CMDomain.Entities
{
    public class SwLogs : CMEntityBase
    {
        public virtual int? Id { get; set; }
        public virtual string? Usuario { get; set; }
        public virtual string? Tipo { get; set; } = "Erro";
        public virtual DateTime? DataHoraCriacao { get; set; }
        public virtual string? BodyRequisicao { get; set; }
        public virtual string? Mensagem { get; set; }

    }
}
