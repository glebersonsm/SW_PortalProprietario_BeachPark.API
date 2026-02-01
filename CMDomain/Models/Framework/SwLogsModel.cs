namespace CMDomain.Models.Framework
{
    public class SwLogsModel
    {
        public int? Id { get; set; }
        public string? UsuarioCriacao { get; set; }
        public DateTime? DataGravacao { get; set; }
        public DateTime? DataHoraEntrada { get; set; }
        public DateTime? DataHoraSaida { get; set; }
        public string? UrlRequested { get; set; }
        public string? ClientIpAddress { get; set; }
        public string? RequestBody { get; set; }
        public string? Response { get; set; }
        public int? StatusResult { get; set; }

    }
}
