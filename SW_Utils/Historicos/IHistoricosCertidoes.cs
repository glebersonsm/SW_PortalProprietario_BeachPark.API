using SW_Utils.Auxiliar;

namespace SW_Utils.Historicos
{
    public interface IHistoricosCertidoes
    {
        List<ParameterValueResult>? GetHistoricos(string nomeTipoHistorico);
    }
}