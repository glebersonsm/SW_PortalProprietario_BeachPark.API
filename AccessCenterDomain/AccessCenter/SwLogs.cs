namespace AccessCenterDomain.AccessCenter
{
    public class SwLogs : EntityBase
    {
        public virtual string? Usuario { get; set; }
        public virtual string? Tipo { get; set; } = "Erro";
        public virtual string? BodyRequisicao { get; set; }
        public virtual string? Mensagem { get; set; }

    }
}
