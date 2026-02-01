namespace CMDomain.Models.Framework
{
    public class SearchLogModel
    {
        public int? Id { get; set; }
        public string? UsuarioCriacao { get; set; }
        public DateTime? DataGeracaoInicial { get; set; }
        public DateTime? DataGeracaoFinal { get; set; }
        public DateTime? DataGravacaoInicial { get; set; }
        public DateTime? DataGravacaoFinal { get; set; }
        public string? RequestBody { get; set; }
        public string? Response { get; set; }
        public int? StatusResult { get; set; }
        public int? QuantidadeRegistrosRetornar { get; set; }

    }
}
