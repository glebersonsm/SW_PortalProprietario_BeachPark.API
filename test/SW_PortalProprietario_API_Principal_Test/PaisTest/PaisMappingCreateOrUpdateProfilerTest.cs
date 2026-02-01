using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Domain.Entities.Core.Geral;

namespace SW_PortalProprietario.Test.PaisTest
{
    public class PaisMappingCreateOrUpdateProfilerTest
    {
        private readonly IProjectObjectMapper _mapper;

        public PaisMappingCreateOrUpdateProfilerTest()
        {
            _mapper = ObjectMapperConfig.Initialize();
        }

        [Fact(DisplayName = "Pais Mapping Model To Entity Create Profiler Test - O campo Id deve ser ignorado")]
        public void DeveIgnorarApenasOIdNaCriacao()
        {
            const string EXPECTED = "0 - BRAsil - 1050";
            var model = new RegistroPaisInputModel()
            {
                Id = 1,
                Nome = "BRAsil",
                CodigoIbge = "1050"
            };

            var dto = _mapper.Map<Pais>(model);

            Assert.Equal(EXPECTED, $"{dto.Id} - {dto.Nome} - {dto.CodigoIbge}");
        }

        [Fact(DisplayName = "Pais Mapping Model To Entity Update Profiler Test - O campo Id e os de auditoria (DataCriacao, UsurioCriacao, DataAlteracao, UsuarioAlteracao) devem ser ignorados")]
        public void DeveIgnorarIdAndCamposDeAuditoria()
        {
            var model = new AlteracaoPaisInputModel()
            {
                Id = 1,
                Nome = "BRAsil",
                CodigoIbge = "2030"
            };

            var entity = new Pais()
            {
                Id = 25,
                Nome = "Autralia",
                CodigoIbge = "2010",
                DataHoraCriacao = DateTime.MinValue,
                UsuarioCriacao = 1,
                DataHoraAlteracao = DateTime.MaxValue,
                UsuarioAlteracao = 2,
                ObjectGuid = Guid.NewGuid().ToString()
            };

            string EXPECTED = $"25 - BRAsil - 2030 - {DateTime.MinValue:dd/MM/yyyy} - {DateTime.MaxValue:dd/MM/yyyy} UC: 1 UA: 2 ObjectGuid: {entity.ObjectGuid}";


            var dto = _mapper.Map(model, entity);

            Assert.Equal(EXPECTED, $"{dto.Id} - {dto.Nome} - {dto.CodigoIbge} - {DateTime.MinValue:dd/MM/yyyy} - {DateTime.MaxValue:dd/MM/yyyy} UC: 1 UA: 2 ObjectGuid: {dto.ObjectGuid}");

        }

    }
}
