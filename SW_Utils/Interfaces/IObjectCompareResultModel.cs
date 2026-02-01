using SW_Utils.Models;

namespace SW_Utils.Interfaces
{
    public interface IObjectCompareResultModel
    {
        DateTime? DataHoraOperacao { get; set; }
        List<AlteracaoResultModel> Modificacoes { get; set; }
        string? ObjectGuid { get; set; }
        int? ObjectId { get; set; }
        string? ObjectType { get; set; }
        string? TipoOperacao { get; set; }
        int? UsuarioOperacao { get; set; }
    }
}
