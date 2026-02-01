using Dapper;
using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models.FrameworkModels;
using SW_PortalProprietario.Application.Models.PessoaModels;
using SW_PortalProprietario.Application.Models.SystemModels;
using SW_PortalProprietario.Application.Services.Core.Auxiliar;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Domain.Entities.Core.Framework;
using SW_Utils.Auxiliar;
using SW_Utils.Functions;
using System.Text;

namespace SW_PortalProprietario.Application.Services.Core
{
    public class FrameworkService : IFrameworkService
    {
        private readonly IRepositoryNH _repository;
        private readonly ILogger<FrameworkService> _logger;
        private readonly IProjectObjectMapper _mapper;
        private readonly IServiceBase _serviceBase;
        public FrameworkService(IRepositoryNH repository,
            ILogger<FrameworkService> logger,
            IProjectObjectMapper mapper,
            IServiceBase serviceBase)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _serviceBase = serviceBase;
        }

        public async Task<GrupoEmpresaModel> DeleteCompanyGroup(int id)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        public async Task<GrupoEmpresaModel> SaveCompanyGroup(RegistroGrupoEmpresaInputModel grupoEmpresa)
        {
            try
            {
                _repository.BeginTransaction();

                var grpEmpresa = await RegistrarAlterarGrupoEmpresa(grupoEmpresa);

                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                {
                    _logger.LogInformation($"GrupoEmpresa: ({grupoEmpresa.Id} - {grupoEmpresa.Pessoa?.RazaoSocial}) salvo com sucesso!");

                    if (grpEmpresa != null)
                        return _mapper.Map(grpEmpresa, new GrupoEmpresaModel());

                }

                throw exception ?? new Exception($"Não foi possível salvar o GrupoEmpresa: ({grupoEmpresa.Pessoa?.RazaoSocial})");
            }
            catch (Exception err)
            {
                _logger.LogError(err, $"Não foi possível salvar o GrupoEmpresa: ({grupoEmpresa.Pessoa?.RazaoSocial})");
                _repository.Rollback();
                throw;
            }
        }

        public async Task<IEnumerable<GrupoEmpresaModel>?> SearchCompanyGroup(GrupoEmpresaSearchModel searchModel)
        {
            List<GrupoEmpresaModel> listGrupoEmpresaRetorno = new List<GrupoEmpresaModel>();

            List<Parameter> parameters = new();

            StringBuilder sb = new(@$"
                        From 
                        GrupoEmpresa ge
                        Inner Join Fetch ge.Pessoa p
                        Where 1 = 1");
            if (!string.IsNullOrEmpty(searchModel.Nome))
                sb.AppendLine($" and Lower(p.Nome) like '%{searchModel.Nome.ToLower()}%'");

            if (!string.IsNullOrEmpty(searchModel.Codigo))
            {
                sb.AppendLine($" and ge.Codigo = :code");
                parameters.Add(new Parameter("code", searchModel.Codigo));
            }

            if (searchModel.Id.GetValueOrDefault(0) > 0)
            {
                sb.AppendLine($" and ge.Id = :grupoEmpresaId");
                parameters.Add(new Parameter("grupoEmpresaId", searchModel.Id));
            }

            if (searchModel.Status.HasValue)
            {
                sb.AppendLine($" and (ge.Status = :status or ge.Status = :statusName)");
                parameters.Add(new Parameter("status", searchModel.Status.GetValueOrDefault()));
                parameters.Add(new Parameter("statusName", searchModel.Status.GetValueOrDefault() == Domain.Enumns.EnumStatus.Ativo ? "Ativo" : "Inativo"));
            }

            var companiesGroup = await _repository.FindByHql<GrupoEmpresa>(sb.ToString(), parameters.ToArray());

            listGrupoEmpresaRetorno = companiesGroup.Any() ? companiesGroup.Select(a => _mapper.Map(a, new GrupoEmpresaModel())).AsList() : new();

            if (listGrupoEmpresaRetorno.Any())
            {
                if (searchModel.PopularEmpresa.GetValueOrDefault(false))
                {
                    await PopulateCompaniesOfGroups(listGrupoEmpresaRetorno);
                }

                if (searchModel.CarregarPessoaCompleta.GetValueOrDefault(false))
                {
                    await PopularDadosAuxiliaresPessoa(listGrupoEmpresaRetorno);
                }
            }

            if (listGrupoEmpresaRetorno.Any())
                return await _serviceBase.SetUserName(listGrupoEmpresaRetorno);

            return default;
        }

        public async Task<IEnumerable<ModuloModel>?> SearchModules(ModuloSearchModel searchModel)
        {
            List<Parameter> parameters = new();
            StringBuilder sb = new(@"Select 
                                    m.Id, 
                                    m.Codigo,
                                    m.Nome,
                                    a.Id as AreaSistema,
                                    a.Nome as AreaSistemaNome,
                                    gm.Id as GrupoModulo,
                                    gm.Nome as GrupoModuloNome
                                    From 
                                    Modulo m
                                    Inner Join GrupoModulo gm on m.GrupoModulo = gm.Id
                                    Inner Join AreaSistema a on m.AreaSistema = a.Id
                                    Where 1 = 1");
            if (!string.IsNullOrEmpty(searchModel.Nome))
                sb.AppendLine($" and Lower(m.Nome) like '{searchModel.Nome.ToLower()}%'");

            if (!string.IsNullOrEmpty(searchModel.Codigo))
            {
                sb.AppendLine($" and m.Codigo = :code");
                parameters.Add(new Parameter("code", searchModel.Codigo));
            }

            if (searchModel.Id.GetValueOrDefault(0) > 0)
            {
                sb.AppendLine($" and m.Id = :id");
                parameters.Add(new Parameter("id", searchModel.Id.GetValueOrDefault()));
            }

            var modules = await _repository.FindBySql<ModuloModel>(sb.ToString(), parameters.ToArray());
            if (searchModel.CarregarPermissoes.GetValueOrDefault(false))
            {
                foreach (var module in modules)
                {
                    module.Permissoes = (await _repository.FindBySql<ModuloPermissaoModel>(@$"Select 
                                    mp.Id, 
                                    p.Nome,
                                    p.NomeInterno,
                                    p.TipoPermissao
                                    From 
                                    ModuloPermissao mp 
                                    Inner Join Modulo m on mp.Modulo = m.Id 
                                    Inner Join Permissao p on mp.Permissao = p.Id 
                                    Where 
                                    m.Id = {module.Id}")).ToList();
                }
            }

            if (modules.Any())
                return await _serviceBase.SetUserName(modules.AsList());

            return default;
        }

        private async Task PopulateCompaniesOfGroups(IList<GrupoEmpresaModel> companiesGroup)
        {
            foreach (var companyGroup in companiesGroup)
            {
                var companies = (await _repository.FindBySql<EmpresaModel>(@$"Select 
                                    c.Id as EmpresaId, 
                                    cp.Id as EmpresaPessoaId,
                                    c.Codigo as EmpesaCodigo,
                                    cp.Nome as EmpresaNome,
                                    ge.Id as GrupoEmpresaId,
                                    ge.Codigo as GrupoEmpresaCodigo,
                                    gep.Id as GrupoEmpresaPessoaId,
                                    gep.Nome as GrupoEmpresaNome,
                                    c.RegimeTributacao as EmpresaRegimeTributacao
                                    From 
                                    Empresa c
                                    Inner Join Pessoa cp on c.Pessoa = cp.Id
                                    Inner Join GrupoEmpresa ge on c.GrupoEmpresa = ge.Id
                                    Inner Join Pessoa gep on ge.Pessoa = gep.Id
                                    Where
                                    ge.Id = {companyGroup.Id}"
                                )).ToList();

                companyGroup.Empresas = companies.Any() ? companies : null;

            }
        }

        public async Task<GrupoEmpresaModel> UpdateCompanyGroup(AlteracaoGrupoEmpresaInputModel model)
        {
            _repository.BeginTransaction();
            try
            {

                var grpEmpresa = await RegistrarAlterarGrupoEmpresa(model);


                await _repository.Save(grpEmpresa);

                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                    return _mapper.Map(grpEmpresa, new GrupoEmpresaModel());
                else throw exception ?? new Exception("Erro na operação");
            }
            catch (Exception)
            {
                _repository.Rollback();
                throw;
            }

        }

        public async Task<EmpresaModel> SaveCompany(RegistroEmpresaInputModel model)
        {
            try
            {
                _repository.BeginTransaction();

                var empresa = await RegistrarAlterarEmpresa(model);

                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                {
                    _logger.LogInformation($"Empresa: ({empresa.Id} - {empresa.Codigo} - {empresa.Pessoa.Nome}) salvo com sucesso!");

                    if (empresa != null)
                        return _mapper.Map(empresa, new EmpresaModel());

                }

                throw exception ?? new Exception($"Não foi possível salvar o Empresa: ({empresa.Codigo} - {empresa.Pessoa?.Nome})");
            }
            catch (Exception err)
            {
                _logger.LogError(err, $"Não foi possível salvar a Empresa: ({model.Pessoa?.RazaoSocial})");
                _repository.Rollback();
                throw;
            }
        }

        public async Task<EmpresaModel> UpdateCompany(AlteracaoEmpresaInputModel model)
        {
            _repository.BeginTransaction();
            try
            {

                var empresa = await RegistrarAlterarEmpresa(model);

                var (executed, exception) = await _repository.CommitAsync();
                if (executed)
                    return _mapper.Map(empresa, new EmpresaModel());
                else throw exception ?? new Exception("Erro na operação");
            }
            catch (Exception)
            {
                _repository.Rollback();
                throw;
            }
        }


        public async Task<IEnumerable<EmpresaModel>?> SearchCompany(EmpresaSearchModel searchModel)
        {
            List<EmpresaModel> listEmpresaRetorno = new List<EmpresaModel>();

            List<Parameter> parameters = new();

            StringBuilder sb = new(@$"
                        From 
                        Empresa e
                        Left Join Fetch e.Pessoa p
                        Left Join Fetch e.GrupoEmpresa ge
                        Left Join Fetch ge.Pessoa gep
                        Where 1 = 1");
            if (!string.IsNullOrEmpty(searchModel.Nome))
                sb.AppendLine($" and Lower(p.Nome) like '%{searchModel.Nome.ToLower()}%'");

            if (!string.IsNullOrEmpty(searchModel.Codigo))
            {
                sb.AppendLine($" and e.Codigo = :code");
                parameters.Add(new Parameter("code", searchModel.Codigo));
            }

            if (searchModel.Id.GetValueOrDefault(0) > 0)
            {
                sb.AppendLine($" and e.Id = :grupoEmpresaId");
                parameters.Add(new Parameter("grupoEmpresaId", searchModel.Id));
            }

            var empresas = await _repository.FindByHql<Empresa>(sb.ToString(), parameters.ToArray());

            listEmpresaRetorno = empresas.Any() ? empresas.Select(a => _mapper.Map(a, new EmpresaModel())).AsList() : new();

            if (listEmpresaRetorno.Any())
            {

                if (searchModel.CarregarPessoaCompleta.GetValueOrDefault(false))
                {
                    await PopularDadosAuxiliaresPessoa(listEmpresaRetorno);
                }
            }

            return listEmpresaRetorno.Any() ? listEmpresaRetorno : null;
        }

        private async Task<GrupoEmpresa?> RegistrarAlterarGrupoEmpresa(RegistroGrupoEmpresaInputModel grupoEmpresaInputModel)
        {
            if (grupoEmpresaInputModel == null)
                throw new Exception("Deve ser enviado os dados do grupo empresa para inclusão/alteração");

            PessoaCompletaModel? pessoaDocumentoInformado = null;

            GrupoEmpresa? grupoEmpresa = null;

            if (grupoEmpresaInputModel.Id.GetValueOrDefault(0) > 0)
            {
                grupoEmpresa = (await _repository.FindByHql<GrupoEmpresa>($"From GrupoEmpresa gu Inner Join Fetch gu.Pessoa p Where gu.Id = {grupoEmpresaInputModel.Id}")).FirstOrDefault();
            }

            if (grupoEmpresa == null)
            {
                grupoEmpresa = _mapper.Map(grupoEmpresaInputModel, grupoEmpresa);
            }
            else
            {
                grupoEmpresa = _mapper.Map(grupoEmpresaInputModel, grupoEmpresa);
            }


            if (grupoEmpresaInputModel?.Pessoa?.Documentos != null)
            {
                foreach (var documento in grupoEmpresaInputModel.Pessoa.Documentos.Where(c => !string.IsNullOrEmpty(c.Numero)))
                {
                    pessoaDocumentoInformado = (await _repository.FindBySql<PessoaCompletaModel>(@$"Select 
                                                                                        p.* 
                                                                                       From 
                                                                                        Pessoa p 
                                                                                        Inner Join PessoaDocumento pd on pd.Pessoa = p.Id 
                                                                                       Where 
                                                                                        pd.TipoDocumento = {documento.TipoDocumentoId.GetValueOrDefault()} and 
                                                                                        (
                                                                                            pd.Numero = '{documento?.Numero?.TrimEnd()}' or 
                                                                                            pd.ValorNumerico = '{(documento?.Numero?.TrimEnd()).RemoveAccents(new List<string>() { ".", "-", "/" })}'                                                                    
                                                                                        )")).FirstOrDefault();

                    if (pessoaDocumentoInformado != null)
                        break;
                }
            }

            if (grupoEmpresaInputModel != null && grupoEmpresaInputModel.Pessoa != null && grupoEmpresaInputModel.Pessoa.Id.GetValueOrDefault(0) == 0 && grupoEmpresa != null && grupoEmpresa.Pessoa.Id > 0)
            {
                grupoEmpresaInputModel.Pessoa.Id = grupoEmpresa.Pessoa.Id;
            }

            var pessoa = grupoEmpresaInputModel?.Pessoa != null && grupoEmpresaInputModel.Pessoa.Id.GetValueOrDefault(0) > 0 ?
                (await _repository.FindByHql<Domain.Entities.Core.DadosPessoa.Pessoa>(@$"From Pessoa p
                                                        Where 
                                                            p.Id = {grupoEmpresaInputModel.Pessoa.Id.GetValueOrDefault()}")).FirstOrDefault() : null;

            Domain.Entities.Core.DadosPessoa.Pessoa pessoaOld = null;

            if (pessoa == null && grupoEmpresaInputModel.Id.GetValueOrDefault(0) > 0)
            {
                grupoEmpresa = (await _repository.FindByHql<GrupoEmpresa>($"From GrupoEmpresa gu Inner Join Fetch gu.Pessoa p Where u.Id = {grupoEmpresaInputModel.Id.GetValueOrDefault()}")).FirstOrDefault();
                if (grupoEmpresa != null)
                    pessoa = grupoEmpresa.Pessoa;
            }


            if (pessoa == null)
            {

                if (pessoaDocumentoInformado != null)
                    throw new ArgumentException($"Um dos documentos informados: '{string.Join(",", grupoEmpresaInputModel.Pessoa.Documentos.Where(c => !string.IsNullOrEmpty(c.Numero)).Select(a => a.Numero?.ToString()).ToList())}'");

                pessoa = _mapper.Map(grupoEmpresaInputModel.Pessoa, pessoa);

            }
            else
            {

                pessoaOld = await _serviceBase.GetObjectOld<Domain.Entities.Core.DadosPessoa.Pessoa>(pessoa.Id);


                if (pessoaDocumentoInformado != null && pessoa.Id != pessoaDocumentoInformado.Id)
                    throw new ArgumentException($"Um dos documentos informados: '{string.Join(",", grupoEmpresaInputModel.Pessoa.Documentos.Where(c => !string.IsNullOrEmpty(c.Numero)).Select(a => a.Numero?.ToString()).ToList())}'");

                if (!string.IsNullOrEmpty(grupoEmpresaInputModel?.Pessoa?.EmailAlternativo) && grupoEmpresaInputModel.Pessoa.EmailAlternativo.Contains("@"))
                {
                    pessoa.EmailAlternativo = grupoEmpresaInputModel.Pessoa.EmailAlternativo.TrimEnd();
                }

                if (!string.IsNullOrEmpty(grupoEmpresaInputModel?.Pessoa?.EmailPreferencial) && grupoEmpresaInputModel.Pessoa.EmailPreferencial.Contains("@"))
                {
                    pessoa.EmailPreferencial = grupoEmpresaInputModel.Pessoa.EmailPreferencial.TrimEnd();
                }
            }

            _serviceBase.Compare(pessoaOld, pessoa);

            await _repository.Save(pessoa);

            if (grupoEmpresaInputModel.Pessoa != null)
                await SincronizarDadosAuxiliaresExecute(grupoEmpresaInputModel.Pessoa?.Enderecos ?? new(), grupoEmpresaInputModel.Pessoa?.Telefones ?? new(), grupoEmpresaInputModel.Pessoa?.Documentos ?? new(), pessoa);

            grupoEmpresa.Pessoa = pessoa;
            await _repository.Save(grupoEmpresa);

            return grupoEmpresa;
        }

        private async Task<GrupoEmpresa?> RegistrarAlterarGrupoEmpresa(AlteracaoGrupoEmpresaInputModel grupoEmpresaInputModel)
        {
            if (grupoEmpresaInputModel == null)
                throw new Exception("Deve ser enviado os dados do grupo usuário para inclusão/alteração");

            PessoaCompletaModel? pessoaDocumentoInformado = null;

            GrupoEmpresa? grupoEmpresa = null;

            if (grupoEmpresaInputModel.Id.GetValueOrDefault(0) > 0)
            {
                grupoEmpresa = (await _repository.FindByHql<GrupoEmpresa>($"From GrupoEmpresa gu Inner Join Fetch gu.Pessoa p Where gu.Id = {grupoEmpresaInputModel.Id}")).FirstOrDefault();
            }

            if (grupoEmpresa == null)
            {
                grupoEmpresa = _mapper.Map(grupoEmpresaInputModel, grupoEmpresa);
            }
            else
            {
                grupoEmpresa = _mapper.Map(grupoEmpresaInputModel, grupoEmpresa);
            }

            if (string.IsNullOrEmpty(grupoEmpresa.Codigo) && grupoEmpresa.Id == 0)
            {
                var baseCodigo = (await _repository.FindBySql<GrupoEmpresaModel>("Select Max(ge.Codigo) as Codigo From GrupoEmpresa ge")).FirstOrDefault();
                if (baseCodigo != null && !string.IsNullOrEmpty(baseCodigo.Codigo))
                    grupoEmpresa.Codigo = Convert.ToString((Convert.ToInt32(baseCodigo.Codigo) + 1)).PadLeft(5, '0');
                else grupoEmpresa.Codigo = "00001";
            }


            if (grupoEmpresaInputModel?.Pessoa?.Documentos != null)
            {
                foreach (var documento in grupoEmpresaInputModel.Pessoa.Documentos.Where(c => !string.IsNullOrEmpty(c.Numero)))
                {
                    pessoaDocumentoInformado = (await _repository.FindBySql<PessoaCompletaModel>(@$"Select 
                                                                                        p.* 
                                                                                       From 
                                                                                        Pessoa p 
                                                                                        Inner Join PessoaDocumento pd on pd.Pessoa = p.Id 
                                                                                       Where 
                                                                                        pd.TipoDocumento = {documento.TipoDocumentoId.GetValueOrDefault()} and 
                                                                                        (
                                                                                            pd.Numero = '{documento?.Numero?.TrimEnd()}' or 
                                                                                            pd.ValorNumerico = '{(documento?.Numero?.TrimEnd()).RemoveAccents(new List<string>() { ".", "-", "/" })}'                                                                    
                                                                                        )")).FirstOrDefault();

                    if (pessoaDocumentoInformado != null)
                        break;
                }
            }

            if (grupoEmpresaInputModel != null && grupoEmpresaInputModel.Pessoa != null && grupoEmpresaInputModel.Pessoa.Id.GetValueOrDefault(0) == 0 && grupoEmpresa != null && grupoEmpresa.Pessoa.Id > 0)
            {
                grupoEmpresaInputModel.Pessoa.Id = grupoEmpresa.Pessoa.Id;
            }

            var pessoa = grupoEmpresaInputModel?.Pessoa != null && grupoEmpresaInputModel.Pessoa.Id.GetValueOrDefault(0) > 0 ?
                (await _repository.FindByHql<Domain.Entities.Core.DadosPessoa.Pessoa>(@$"From Pessoa p
                                                        Where 
                                                            p.Id = {grupoEmpresaInputModel.Pessoa.Id.GetValueOrDefault()}")).FirstOrDefault() : null;

            Domain.Entities.Core.DadosPessoa.Pessoa pessoaOld = null;

            if (pessoa == null && grupoEmpresaInputModel.Id.GetValueOrDefault(0) > 0)
            {
                grupoEmpresa = (await _repository.FindByHql<GrupoEmpresa>($"From GrupoEmpresa gu Inner Join Fetch gu.Pessoa p Where u.Id = {grupoEmpresaInputModel.Id.GetValueOrDefault()}")).FirstOrDefault();
                if (grupoEmpresa != null)
                    pessoa = grupoEmpresa.Pessoa;
            }


            if (pessoa == null)
            {

                if (pessoaDocumentoInformado != null)
                    throw new ArgumentException($"Um dos documentos informados: '{string.Join(",", grupoEmpresaInputModel.Pessoa.Documentos.Where(c => !string.IsNullOrEmpty(c.Numero)).Select(a => a.Numero?.ToString()).ToList())}' já está sendo utilizado por outra pessoa de Id: {pessoaDocumentoInformado.Id}");

                pessoa = _mapper.Map(grupoEmpresaInputModel.Pessoa, pessoa);

            }
            else
            {

                pessoaOld = await _serviceBase.GetObjectOld<Domain.Entities.Core.DadosPessoa.Pessoa>(pessoa.Id);

                if (grupoEmpresaInputModel.Pessoa != null)
                {
                    pessoa = _mapper.Map(grupoEmpresaInputModel.Pessoa, pessoa);
                }

                if (pessoaDocumentoInformado != null && pessoa.Id != pessoaDocumentoInformado.Id)
                    throw new ArgumentException($"Um dos documentos informados: '{string.Join(",", grupoEmpresaInputModel.Pessoa.Documentos.Where(c => !string.IsNullOrEmpty(c.Numero)).Select(a => a.Numero?.ToString()).ToList())}' já está sendo utilizado por outra pessoa de Id: {pessoaDocumentoInformado.Id}");

            }

            await _repository.Save(pessoa);

            _serviceBase.Compare(pessoaOld, pessoa);

            await SincronizarDadosAuxiliaresExecute(grupoEmpresaInputModel.Pessoa.Enderecos, grupoEmpresaInputModel.Pessoa.Telefones, grupoEmpresaInputModel.Pessoa.Documentos, pessoa);


            grupoEmpresa.Pessoa = pessoa;
            await _repository.Save(grupoEmpresa);

            return grupoEmpresa;
        }

        private async Task<Empresa?> RegistrarAlterarEmpresa(RegistroEmpresaInputModel empresaInputModel)
        {
            if (empresaInputModel == null)
                throw new Exception("Deve ser enviado os dados da empresa para inclusão/alteração");

            var gruposEmpresas = (await _repository.FindByHql<GrupoEmpresa>("From GrupoEmpresa ge Inner Join Fetch ge.Pessoa p")).AsList();

            PessoaCompletaModel? pessoaDocumentoInformado = null;

            Empresa? empresa = null;

            if (empresaInputModel.Id.GetValueOrDefault(0) > 0)
            {
                empresa = (await _repository.FindByHql<Empresa>($"From Empresa e Inner Join Fetch e.Pessoa p Left Join Fetch e.GrupoEmpresa gu Left Join Fetch gu.Pessoa gup Where e.Id = {empresaInputModel.Id}")).FirstOrDefault();
                if (empresaInputModel.GrupoEmpresaId.GetValueOrDefault(0) == 0)
                {
                    if (empresa != null && empresa.GrupoEmpresa != null)
                        empresaInputModel.GrupoEmpresaId = empresa.GrupoEmpresa.Id;
                }
            }

            if (empresaInputModel.GrupoEmpresaId.GetValueOrDefault(0) == 0)
            {
                if (gruposEmpresas != null && gruposEmpresas.Any() && gruposEmpresas.Count() == 1)
                    empresaInputModel.GrupoEmpresaId = gruposEmpresas.First().Id;
            }

            if (empresa == null)
            {
                empresa = _mapper.Map(empresaInputModel, empresa);
            }
            else
            {
                empresa = _mapper.Map(empresaInputModel, empresa);
            }

            if (empresa.GrupoEmpresa != null && empresa.GrupoEmpresa.Id == 0)
                empresa.GrupoEmpresa = null;

            if (string.IsNullOrEmpty(empresa.Codigo) && empresa.Id == 0)
            {
                var baseCodigo = (await _repository.FindBySql<EmpresaModel>("Select Max(e.Codigo) as Codigo From Empresa e")).FirstOrDefault();
                if (baseCodigo != null && !string.IsNullOrEmpty(baseCodigo.Codigo))
                    empresa.Codigo = Convert.ToString((Convert.ToInt32(baseCodigo.Codigo) + 1)).PadLeft(5, '0');
                else empresa.Codigo = "00001";
            }


            if (empresaInputModel?.Pessoa?.Documentos != null)
            {
                foreach (var documento in empresaInputModel.Pessoa.Documentos.Where(c => !string.IsNullOrEmpty(c.Numero)))
                {
                    pessoaDocumentoInformado = (await _repository.FindBySql<PessoaCompletaModel>(@$"Select 
                                                                                        p.* 
                                                                                       From 
                                                                                        Pessoa p 
                                                                                        Inner Join PessoaDocumento pd on pd.Pessoa = p.Id 
                                                                                       Where 
                                                                                        pd.TipoDocumento = {documento.TipoDocumentoId.GetValueOrDefault()} and 
                                                                                        (
                                                                                            pd.Numero = '{documento?.Numero?.TrimEnd()}' or 
                                                                                            pd.ValorNumerico = '{(documento?.Numero?.TrimEnd()).RemoveAccents(new List<string>() { ".", "-", "/" })}'                                                                    
                                                                                        )")).FirstOrDefault();

                    if (pessoaDocumentoInformado != null)
                        break;
                }
            }

            if (empresaInputModel != null && empresaInputModel.Pessoa != null && empresaInputModel.Pessoa.Id.GetValueOrDefault(0) == 0 && empresa != null && empresa.Pessoa.Id > 0)
            {
                empresaInputModel.Pessoa.Id = empresa.Pessoa.Id;
            }

            var pessoa = empresaInputModel?.Pessoa != null && empresaInputModel.Pessoa.Id.GetValueOrDefault(0) > 0 ?
                (await _repository.FindByHql<Domain.Entities.Core.DadosPessoa.Pessoa>(@$"From Pessoa p
                                                        Where 
                                                            p.Id = {empresaInputModel.Pessoa.Id.GetValueOrDefault()}")).FirstOrDefault() : null;

            Domain.Entities.Core.DadosPessoa.Pessoa pessoaOld = null;

            if (pessoa == null && empresaInputModel.Id.GetValueOrDefault(0) > 0)
            {
                empresa = (await _repository.FindByHql<Empresa>($"From Empresa gu Inner Join Fetch gu.Pessoa p Where u.Id = {empresaInputModel.Id.GetValueOrDefault()}")).FirstOrDefault();
                if (empresa != null)
                    pessoa = empresa.Pessoa;
            }


            if (pessoa == null)
            {

                if (pessoaDocumentoInformado != null)
                    throw new ArgumentException($"Um dos documentos informados: '{string.Join(",", empresaInputModel.Pessoa.Documentos.Where(c => !string.IsNullOrEmpty(c.Numero)).Select(a => a.Numero?.ToString()).ToList())}' já está sendo utilizado por outra pessoa de Id: {pessoaDocumentoInformado.Id}");

                pessoa = _mapper.Map(empresaInputModel.Pessoa, pessoa);

            }
            else
            {

                pessoaOld = await _serviceBase.GetObjectOld<Domain.Entities.Core.DadosPessoa.Pessoa>(pessoa.Id);

                if (empresaInputModel.Pessoa != null)
                {
                    pessoa = _mapper.Map(empresaInputModel.Pessoa, pessoa);
                }


                if (pessoaDocumentoInformado != null && pessoa.Id != pessoaDocumentoInformado.Id)
                    throw new ArgumentException($"Um dos documentos informados: '{string.Join(",", empresaInputModel.Pessoa.Documentos.Where(c => !string.IsNullOrEmpty(c.Numero)).Select(a => a.Numero?.ToString()).ToList())}' já está sendo utilizado por outra pessoa de Id: {pessoaDocumentoInformado.Id}");

            }

            _serviceBase.Compare(pessoaOld, pessoa);

            await _repository.Save(pessoa);

            await SincronizarDadosAuxiliaresExecute(empresaInputModel.Pessoa?.Enderecos ?? new(), empresaInputModel.Pessoa?.Telefones ?? new(), empresaInputModel.Pessoa?.Documentos ?? new(), pessoa);

            empresa.Pessoa = pessoa;
            await _repository.Save(empresa);

            return empresa;
        }

        private async Task<Empresa?> RegistrarAlterarEmpresa(AlteracaoEmpresaInputModel empresaInputModel)
        {
            if (empresaInputModel == null)
                throw new Exception("Deve ser enviado os dados da empresa para inclusão/alteração");

            PessoaCompletaModel? pessoaDocumentoInformado = null;

            Empresa? empresa = null;

            if (empresaInputModel.Id.GetValueOrDefault(0) > 0)
            {
                empresa = (await _repository.FindByHql<Empresa>($"From Empresa e Inner Join Fetch e.Pessoa p Left Join Fetch e.GrupoEmpresa ge Left Join Fetch ge.Pessoa pge Where e.Id = {empresaInputModel.Id}")).FirstOrDefault();
            }

            empresa = _mapper.Map(empresaInputModel, empresa);


            if (empresaInputModel?.Pessoa?.Documentos != null)
            {
                foreach (var documento in empresaInputModel.Pessoa.Documentos.Where(c => !string.IsNullOrEmpty(c.Numero)))
                {
                    pessoaDocumentoInformado = (await _repository.FindBySql<PessoaCompletaModel>(@$"Select 
                                                                                        p.* 
                                                                                       From 
                                                                                        Pessoa p 
                                                                                        Inner Join PessoaDocumento pd on pd.Pessoa = p.Id 
                                                                                       Where 
                                                                                        pd.TipoDocumento = {documento.TipoDocumentoId.GetValueOrDefault()} and 
                                                                                        (
                                                                                            pd.Numero = '{documento?.Numero?.TrimEnd()}' or 
                                                                                            pd.ValorNumerico = '{(documento?.Numero?.TrimEnd()).RemoveAccents(new List<string>() { ".", "-", "/" })}'                                                                    
                                                                                        )")).FirstOrDefault();

                    if (pessoaDocumentoInformado != null)
                        break;
                }
            }

            if (empresaInputModel != null && empresaInputModel.Pessoa != null && empresaInputModel.Pessoa.Id.GetValueOrDefault(0) == 0 && empresa != null && empresa.Pessoa.Id > 0)
            {
                empresaInputModel.Pessoa.Id = empresa.Pessoa.Id;
            }


            Domain.Entities.Core.DadosPessoa.Pessoa pessoaOld = null;

            if (empresa != null && empresa.Id > 0 && empresa.Pessoa == null)
            {
                empresa.Pessoa = (await _repository.FindByHql<Domain.Entities.Core.DadosPessoa.Pessoa>($"From Pessoa p Where p.Id = (Select p1.Id From Empresa e Where e.Id = {empresaInputModel.Id.GetValueOrDefault()}")).FirstOrDefault();
            }


            pessoaOld = empresa.Pessoa != null ? await _serviceBase.GetObjectOld<Domain.Entities.Core.DadosPessoa.Pessoa>(empresa.Pessoa.Id) : null;


            if (pessoaDocumentoInformado != null && empresa?.Pessoa.Id != pessoaDocumentoInformado.Id)
                throw new ArgumentException($"Um dos documentos informados: '{string.Join(",", empresaInputModel.Pessoa.Documentos.Where(c => !string.IsNullOrEmpty(c.Numero)).Select(a => a.Numero?.ToString()).ToList())}' já está sendo utilizado por outra pessoa de Id: {pessoaDocumentoInformado.Id}");

            if (empresa?.Pessoa != null)
            {
                await _repository.Save(empresa.Pessoa);

                _serviceBase.Compare(pessoaOld, empresa.Pessoa);
            }

            await SincronizarDadosAuxiliaresExecute(empresaInputModel.Pessoa?.Enderecos, empresaInputModel.Pessoa?.Telefones, empresaInputModel.Pessoa?.Documentos, empresa.Pessoa);

            if (empresa?.Pessoa == null)
                throw new ArgumentException("Não foi informada a pessoa na empresa!");

            await _repository.Save(empresa);

            return empresa;
        }

        private async Task SincronizarDadosAuxiliaresExecute(List<PessoaEnderecoInputModel> enderecos, List<PessoaTelefoneInputModel> telefones, List<PessoaDocumentoInputModel> documentos, Domain.Entities.Core.DadosPessoa.Pessoa? pessoa)
        {
            var pessoaSincronizacaoAuxiliar = new PessoaSincronizacaoListasAuxiliar(_repository, _logger, _serviceBase, _mapper);


            if (enderecos != null && enderecos.Any())
                await pessoaSincronizacaoAuxiliar.SincronizarEnderecos(pessoa, enderecos.ToArray());

            if (telefones != null && telefones.Any())
                await pessoaSincronizacaoAuxiliar.SincronizarTelefones(pessoa, telefones.ToArray());

            if (documentos != null && documentos.Any())
                await pessoaSincronizacaoAuxiliar.SincronizarDocumentos(pessoa, false, documentos.ToArray());
        }

        private async Task PopularDadosAuxiliaresPessoa(List<GrupoEmpresaModel> grupoEmpresas)
        {
            if (grupoEmpresas == null || !grupoEmpresas.Any(c => c.Pessoa.Id > 0))
            {
                await Task.CompletedTask;
                return;
            }

            foreach (var item in grupoEmpresas)
            {
                item.Pessoa.Telefones = (await _repository.FindBySql<PessoaTelefoneModel>(@$"Select 
																							pt.Id,
																							pt.DataHoraCriacao,
																							pt.UsuarioCriacao,
																							pt.DataHoraAlteracao,
																							pt.UsuarioAlteracao,
																							pt.Pessoa, 
																							pt.TipoTelefone as TipoTelefoneId, 
																							tt.Nome as TipoTelefoneNome,
																							pt.Numero,
																						    tt.Mascara as TipoTelefoneMascara,
																							Coalesce(pt.Preferencial,0) AS Preferencial,
                                                                                            pt.NumeroFormatado
																							From 
																							PessoaTelefone pt 
																							Inner Join TipoTelefone tt on pt.TipoTelefone = tt.Id
																							Where 
																							pt.Pessoa = {item.Pessoa.Id}")).AsList();


                item.Pessoa.Enderecos = (await _repository.FindBySql<PessoaEnderecoModel>(@$"Select
																							pt.Id,
																							pt.UsuarioCriacao,
																						    pt.DataHoraCriacao,
																							pt.UsuarioAlteracao,
																							pt.DataHoraAlteracao,
																							pt.Pessoa,
																							c.Id as CidadeId,
																					        c.Nome as CidadeNome,
																							e.Id as EstadoId,
																							e.Sigla as EstadoSigla,
																							e.Nome as EstadoNome,
																							pt.TipoEndereco as TipoEnderecoId, 
																							tt.Nome as TipoEnderecoNome,
																							pt.Numero,
																							pt.Logradouro,
																							pt.Bairro,
																							pt.Complemento,
																							pt.Cep,
																							Coalesce(pt.Preferencial,0) AS Preferencial
																							From 
																							PessoaEndereco pt 
																							Inner Join Cidade c ON pt.Cidade = c.Id
																							Inner Join Estado e on c.Estado = e.Id
																							Inner Join TipoEndereco tt on pt.TipoEndereco = tt.Id
																							Where 
																							pt.Pessoa = {item.Pessoa.Id}")).AsList();


                item.Pessoa.Documentos = (await _repository.FindBySql<PessoaDocumentoModel>(@$"Select
																							pd.Id,
																							pd.UsuarioCriacao,
																						    pd.DataHoraCriacao,
																							pd.UsuarioAlteracao,
																							pd.DataHoraAlteracao,
																							pd.Pessoa,
																							td.Id as TipoDocumentoId, 
																							td.Nome as TipoDocumentoNome,
																							pd.Numero,
																							pd.OrgaoEmissor,
																							pd.DataEmissao,
																							pd.DataValidade,
																							td.Mascara as TipoDocumentoMascara,
                                                                                            pd.NumeroFormatado
																							From 
																						    PessoaDocumento pd
																							Inner Join TipoDocumentoPessoa td on pd.TipoDocumento = td.Id
																							Where 
																							pd.Pessoa = {item.Pessoa.Id}")).AsList();


            }

            await Task.CompletedTask;

        }

        private async Task PopularDadosAuxiliaresPessoa(List<EmpresaModel> empresas)
        {
            if (empresas == null || !empresas.Any(c => c.PessoaEmpresa.Id > 0))
            {
                await Task.CompletedTask;
                return;
            }

            foreach (var item in empresas)
            {
                if (item.PessoaEmpresa != null && item.PessoaEmpresa.Id > 0)
                {
                    item.PessoaEmpresa.Telefones = (await _repository.FindBySql<PessoaTelefoneModel>(@$"Select 
																							pt.Id,
																							pt.DataHoraCriacao,
																							pt.UsuarioCriacao,
																							pt.DataHoraAlteracao,
																							pt.UsuarioAlteracao,
																							pt.Pessoa, 
																							pt.TipoTelefone as TipoTelefoneId, 
																							tt.Nome as TipoTelefoneNome,
																							pt.Numero,
																						    tt.Mascara as TipoTelefoneMascara,
																							Coalesce(pt.Preferencial,0) AS Preferencial,
                                                                                            pt.NumeroFormatado
																							From 
																							PessoaTelefone pt 
																							Inner Join TipoTelefone tt on pt.TipoTelefone = tt.Id
																							Where 
																							pt.Pessoa = {item.PessoaEmpresa.Id}")).AsList();


                    item.PessoaEmpresa.Enderecos = (await _repository.FindBySql<PessoaEnderecoModel>(@$"Select
																							pt.Id,
																							pt.UsuarioCriacao,
																						    pt.DataHoraCriacao,
																							pt.UsuarioAlteracao,
																							pt.DataHoraAlteracao,
																							pt.Pessoa,
																							c.Id as CidadeId,
																					        c.Nome as CidadeNome,
																							e.Id as EstadoId,
																							e.Sigla as EstadoSigla,
																							e.Nome as EstadoNome,
																							pt.TipoEndereco as TipoEnderecoId, 
																							tt.Nome as TipoEnderecoNome,
																							pt.Numero,
																							pt.Logradouro,
																							pt.Bairro,
																							pt.Complemento,
																							pt.Cep,
																							Coalesce(pt.Preferencial,0) AS Preferencial
																							From 
																							PessoaEndereco pt 
																							Inner Join Cidade c ON pt.Cidade = c.Id
																							Inner Join Estado e on c.Estado = e.Id
																							Inner Join TipoEndereco tt on pt.TipoEndereco = tt.Id
																							Where 
																							pt.Pessoa = {item.PessoaEmpresa.Id}")).AsList();


                    item.PessoaEmpresa.Documentos = (await _repository.FindBySql<PessoaDocumentoModel>(@$"Select
																							pd.Id,
																							pd.UsuarioCriacao,
																						    pd.DataHoraCriacao,
																							pd.UsuarioAlteracao,
																							pd.DataHoraAlteracao,
																							pd.Pessoa,
																							td.Id as TipoDocumentoId, 
																							td.Nome as TipoDocumentoNome,
																							pd.Numero,
																							pd.OrgaoEmissor,
																							pd.DataEmissao,
																							pd.DataValidade,
																							td.Mascara as TipoDocumentoMascara,
                                                                                            pd.NumeroFormatado
																							From 
																						    PessoaDocumento pd
																							Inner Join TipoDocumentoPessoa td on pd.TipoDocumento = td.Id
																							Where 
																							pd.Pessoa = {item.PessoaEmpresa.Id}")).AsList();


                }

                if (item.PessoaGrupoEmpresa != null && item.PessoaGrupoEmpresa.Id > 0)
                {
                    item.PessoaGrupoEmpresa.Telefones = (await _repository.FindBySql<PessoaTelefoneModel>(@$"Select 
																							pt.Id,
																							pt.DataHoraCriacao,
																							pt.UsuarioCriacao,
																							pt.DataHoraAlteracao,
																							pt.UsuarioAlteracao,
																							pt.Pessoa, 
																							pt.TipoTelefone as TipoTelefoneId, 
																							tt.Nome as TipoTelefoneNome,
																							pt.Numero,
																						    tt.Mascara as TipoTelefoneMascara,
																							Coalesce(pt.Preferencial,0) AS Preferencial,
                                                                                            pt.NumeroFormatado
																							From 
																							PessoaTelefone pt 
																							Inner Join TipoTelefone tt on pt.TipoTelefone = tt.Id
																							Where 
																							pt.Pessoa = {item.PessoaGrupoEmpresa.Id}")).AsList();


                    item.PessoaGrupoEmpresa.Enderecos = (await _repository.FindBySql<PessoaEnderecoModel>(@$"Select
																							pt.Id,
																							pt.UsuarioCriacao,
																						    pt.DataHoraCriacao,
																							pt.UsuarioAlteracao,
																							pt.DataHoraAlteracao,
																							pt.Pessoa,
																							c.Id as CidadeId,
																					        c.Nome as CidadeNome,
																							e.Id as EstadoId,
																							e.Sigla as EstadoSigla,
																							e.Nome as EstadoNome,
																							pt.TipoEndereco as TipoEnderecoId, 
																							tt.Nome as TipoEnderecoNome,
																							pt.Numero,
																							pt.Logradouro,
																							pt.Bairro,
																							pt.Complemento,
																							pt.Cep,
																							Coalesce(pt.Preferencial,0) AS Preferencial
																							From 
																							PessoaEndereco pt 
																							Inner Join Cidade c ON pt.Cidade = c.Id
																							Inner Join Estado e on c.Estado = e.Id
																							Inner Join TipoEndereco tt on pt.TipoEndereco = tt.Id
																							Where 
																							pt.Pessoa = {item.PessoaGrupoEmpresa.Id}")).AsList();


                    item.PessoaGrupoEmpresa.Documentos = (await _repository.FindBySql<PessoaDocumentoModel>(@$"Select
																							pd.Id,
																							pd.UsuarioCriacao,
																						    pd.DataHoraCriacao,
																							pd.UsuarioAlteracao,
																							pd.DataHoraAlteracao,
																							pd.Pessoa,
																							td.Id as TipoDocumentoId, 
																							td.Nome as TipoDocumentoNome,
																							pd.Numero,
																							pd.OrgaoEmissor,
																							pd.DataEmissao,
																							pd.DataValidade,
																							td.Mascara as TipoDocumentoMascara,
                                                                                            pd.NumeroFormatado
																							From 
																						    PessoaDocumento pd
																							Inner Join TipoDocumentoPessoa td on pd.TipoDocumento = td.Id
																							Where 
																							pd.Pessoa = {item.PessoaGrupoEmpresa.Id}")).AsList();


                }
            }


            await Task.CompletedTask;

        }

        public async Task<List<EmpresaVinculadaModel>?> GetEmpresasVinculadas()
        {
            return await _serviceBase.GetEmpresasVinculadas();
        }
    }
}
