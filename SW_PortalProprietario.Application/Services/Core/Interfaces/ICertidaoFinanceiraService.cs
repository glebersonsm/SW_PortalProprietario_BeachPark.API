using SW_PortalProprietario.Application.Models.GeralModels;

namespace SW_PortalProprietario.Application.Services.Core.Interfaces
{
    public interface ICertidaoFinanceiraService
    {
        Task<List<FileResultModel>> GerarCertidaoNegativaPositivaDeDebitosFinanceiros(GeracaoCertidaoInputModel geracaoCertidaoInputModel);
        Task<CertidaoViewModel?> ValidarCertidao(string protocolo);
    }
}
