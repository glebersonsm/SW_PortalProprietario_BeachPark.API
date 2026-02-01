using SW_Utils.Interfaces;

namespace SW_Utils.Models
{
    public class ObjectCompareResultModel : IObjectCompareResultModel
    {
        public string? ObjectType { get; set; }
        public string? ObjectGuid { get; set; }
        public int? ObjectId { get; set; }
        public DateTime? DataHoraOperacao { get; set; }
        public int? UsuarioOperacao { get; set; }
        public string? TipoOperacao { get; set; }
        public List<AlteracaoResultModel> Modificacoes { get; set; } = new List<AlteracaoResultModel>();
    }

}
