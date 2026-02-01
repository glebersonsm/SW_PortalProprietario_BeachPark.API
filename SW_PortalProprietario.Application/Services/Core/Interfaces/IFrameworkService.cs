using SW_PortalProprietario.Application.Models.FrameworkModels;
using SW_PortalProprietario.Application.Models.SystemModels;

namespace SW_PortalProprietario.Application.Services.Core.Interfaces
{
    public interface IFrameworkService
    {
        #region Grupo empresa 
        Task<GrupoEmpresaModel> SaveCompanyGroup(RegistroGrupoEmpresaInputModel model);
        Task<GrupoEmpresaModel> UpdateCompanyGroup(AlteracaoGrupoEmpresaInputModel model);
        Task<IEnumerable<GrupoEmpresaModel>?> SearchCompanyGroup(GrupoEmpresaSearchModel searchModel);

        #endregion

        #region Empresa 
        Task<EmpresaModel> SaveCompany(RegistroEmpresaInputModel model);
        Task<EmpresaModel> UpdateCompany(AlteracaoEmpresaInputModel model);
        Task<IEnumerable<EmpresaModel>?> SearchCompany(EmpresaSearchModel searchModel);

        #endregion

        Task<IEnumerable<ModuloModel>?> SearchModules(ModuloSearchModel searchModel);
        Task<List<EmpresaVinculadaModel>?> GetEmpresasVinculadas();
    }
}
