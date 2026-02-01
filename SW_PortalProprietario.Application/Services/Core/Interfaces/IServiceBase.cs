using Microsoft.AspNetCore.Http;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Domain.Entities.Core;

namespace SW_PortalProprietario.Application.Services.Core.Interfaces
{
    public interface IServiceBase
    {
        Task<T?> GetObjectOld<T>(T entity) where T : EntityBaseCore;
        Task<T?> GetObjectOld<T>(int id) where T : EntityBaseCore;
        void Compare(EntityBaseCore? objOld, EntityBaseCore? newObject);
        Task AddLogAuditoriaMessageToQueue(HttpContext httpContext, int status);
        string RequestArguments { get; set; }
        string ResponseData { get; set; }
        int? UsuarioId { get; set; }
        Task<List<T>> SetUserName<T>(List<T> models) where T : ModelBase;
        Task<T> SetUserName<T>(T models) where T : ModelBase;
        Task<PessoaSistemaXProviderModel?> GetPessoaSistemaVinculadaPessoaProvider(string pessoaSistema, string? providerName = null);
        Task<PessoaSistemaXProviderModel?> GetPessoaProviderVinculadaPessoaSistema(string pessoaProvider, string? providerName = null);
        Task<PessoaSistemaXProviderModel?> GetPessoaProviderVinculadaUsuarioSistema(int usuarioSistemaId, string? providerName = null);
        Task<ParametroSistemaViewModel?> GetParametroSistema(string? pessoaProviderId = null);
        Task<List<EmpresaVinculadaModel>?> GetEmpresasVinculadas();
        Task<List<DadosContratoModel>?> GetContratos(List<int> pessoasPesquisar);
        public string? GetProviderName { get; }
        Task<string> getToken();
    }
}
