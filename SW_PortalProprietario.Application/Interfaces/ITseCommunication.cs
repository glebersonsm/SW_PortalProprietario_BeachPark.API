using Refit;
using SW_PortalProprietario.Application.Models.Tse;

namespace SW_PortalProprietario.Application.Interfaces
{
    public interface ITseCommunication
    {
        [Get("/api/cadastros/GetPessoa/{idpessoa}")]
        [Headers("Authorization: Bearer")]
        Task<TseCustomerModel> GetCustomersAsync(string idPessoa);

        [Get("/api/cadastros/GetPessoaCpf/{cpf}")]
        [Headers("Authorization: Bearer")]
        Task<TseCustomerModel> GetCustomersByCpfAsync(string cpf);

        [Post("/api/Relatorios/ObterJsonDadosRelatorioComParametros/18")]
        [Headers("Authorization: Bearer", "Content-Type: application/json")]
        Task<IEnumerable<TseCustomerModel>> GetCustomers([Body(BodySerializationMethod.Serialized)] List<SW_Utils.Auxiliar.TseParametro> par);

        [Post("/api/Relatorios/ObterJsonDadosRelatorioComParametros/113")]
        [Headers("Authorization: Bearer", "Content-Type: application/json")]
        Task<IEnumerable<TseCustomerModel>> GetContractByCpf([Body(BodySerializationMethod.Serialized)] List<SW_Utils.Auxiliar.TseParametro> par);

    }
}
