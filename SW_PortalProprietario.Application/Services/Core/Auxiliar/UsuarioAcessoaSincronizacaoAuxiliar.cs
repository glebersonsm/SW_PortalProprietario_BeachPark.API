using Dapper;
using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Domain.Entities.Core.Sistema;

namespace SW_PortalProprietario.Application.Services.Core.Auxiliar
{
    public class UsuarioAcessoaSincronizacaoAuxiliar
    {
        private readonly IRepositoryNH _repository;
        private readonly ILogger _logger;
        public UsuarioAcessoaSincronizacaoAuxiliar(IRepositoryNH repository,
            ILogger logger)
        {
            _repository = repository;
            _logger = logger;
        }


        public async Task SincronizarEmpresas(List<int> empresasIdsList, Usuario? usuario, bool removerOutras = false)
        {
            if (usuario == null)
                throw new ArgumentNullException("Deve ser informado o usuário para sincronização de empresas que ele acessa");

            List<EmpresaUsuario> listaSalvar = new List<EmpresaUsuario>();
            var empresasVinculadas = (await _repository.FindByHql<EmpresaUsuario>($"From EmpresaUsuario eu Inner Join Fetch eu.Usuario u Inner Join Fetch eu.Empresa emp Where eu.Usuario = {usuario.Id}")).AsList();
            if (empresasIdsList != null && empresasIdsList.Any())
            {
                foreach (var item in empresasIdsList)
                {
                    if (!empresasVinculadas.Any(c => c.Empresa?.Id == item))
                        listaSalvar.Add(new EmpresaUsuario() { Empresa = new Domain.Entities.Core.Framework.Empresa() { Id = item }, Usuario = new Usuario() { Id = usuario.Id } });

                }
            }

            if (listaSalvar.Any())
                await _repository.SaveRange(listaSalvar);

            if (removerOutras)
            {
                foreach (var empresaUsuario in empresasVinculadas)
                {
                    if (empresasIdsList == null)
                        empresasIdsList = new List<int>();

                    if (!empresasIdsList.Any(c => c == empresaUsuario.Empresa?.Id))
                        _repository.Remove(empresaUsuario);

                }
            }
        }

        public async Task SincronizarGruposUsuarios(List<int> grupoUsuariosIdsList, Usuario? usuario, bool removerOutros = false)
        {
            if (usuario == null)
                throw new ArgumentNullException("Deve ser informado o usuário para sincronização dos grupos de usuários dele");

            List<UsuarioGrupoUsuario> listaSalvar = new List<UsuarioGrupoUsuario>();
            var grupoUsuarioVinculados = (await _repository.FindByHql<UsuarioGrupoUsuario>($"From UsuarioGrupoUsuario ugu Inner Join Fetch ugu.Usuario u Inner Join Fetch ugu.GrupoUsuario gu Where ugu.Usuario = {usuario.Id} and u.DataHoraRemocao is null and coalesce(u.Removido,0) = 0 ")).AsList();
            if (grupoUsuariosIdsList != null && grupoUsuariosIdsList.Any())
            {
                foreach (var item in grupoUsuariosIdsList)
                {
                    if (!grupoUsuarioVinculados.Any(c => c.GrupoUsuario?.Id == item))
                        listaSalvar.Add(new UsuarioGrupoUsuario() { GrupoUsuario = new GrupoUsuario() { Id = item }, Usuario = new Usuario() { Id = usuario.Id } });

                }
            }

            if (listaSalvar.Any())
                await _repository.SaveRange(listaSalvar);

            if (removerOutros)
            {
                foreach (var grupoUsuario in grupoUsuarioVinculados)
                {
                    if (grupoUsuariosIdsList == null)
                        grupoUsuariosIdsList = new List<int>();

                    if (!grupoUsuariosIdsList.Any(c => c == grupoUsuario.GrupoUsuario?.Id))
                        _repository.Remove(grupoUsuario);

                }
            }
        }

    }
}
