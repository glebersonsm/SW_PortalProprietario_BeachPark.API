namespace SW_Utils.Models
{
    public class OperationSystemLogModelEvent
    {
        public OperationSystemLogModelEvent()
        {
            Guid = Guid.NewGuid();
        }
        public Guid Guid { get; }
        public int? UsuarioId { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFinal { get; set; }
        public string? UrlRequested { get; set; }
        public string? ClientIpAddress { get; set; }
        public string? RequestBody { get; set; }
        public string? Response { get; set; }
        public int? StatusResult { get; set; }
        public List<ObjectCompareResultModel> Modificacoes { get; set; } = new List<ObjectCompareResultModel>();
    }

}
