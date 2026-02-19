using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Domain.Entities.Core.Geral;

namespace SW_PortalProprietario.Test.CidadeTest
{
    public class CidadeMappingCreateOrUpdateProfilerTest
    {
        private readonly IProjectObjectMapper _mapper;

        public CidadeMappingCreateOrUpdateProfilerTest()
        {
            _mapper = ObjectMapperConfig.Initialize();
        }

        [Fact(DisplayName = "Cidade Mapping Model To Entity Create Profiler Test - Id deve ser ignorado e as demais propriedades preechidas normalmente")]
        public void DeveIgnorarApenasOIdNaCriacao()
        {
            const string EXPECTED = "0 - Morrinhos - 521080";
            var model = new RegistroCidadeInputModel()
            {
                Id = 1,
                Nome = "Morrinhos",
                CodigoIbge = "521080"
            };

            var dto = _mapper.Map<Cidade>(model);

            Assert.Equal(EXPECTED, $"{dto.Id} - {dto.Nome} - {dto.CodigoIbge}");
        }

        [Fact(DisplayName = "Cidade Mapping Model To Entity Update Profiler Test - Id deve ser ignorado e as demais propriedades preechidas normalmente")]
        public void DeveIgnorarIdAndCamposDeAuditoriaNaAlteracao()
        {
            var model = new AlteracaoCidadeInputModel()
            {
                Id = 1,
                Nome = "Morrinhos",
                CodigoIbge = "521080"
            };

            var entity = new Cidade()
            {
                Id = 25,
                Nome = "Rio de Janeiro",
                CodigoIbge = "201080",
                DataHoraCriacao = DateTime.MinValue,
                UsuarioCriacao = 1,
                DataHoraAlteracao = DateTime.MaxValue,
                UsuarioAlteracao = 2,
                ObjectGuid = Guid.NewGuid().ToString()
            };

            string EXPECTED = $"25 - Morrinhos - 521080 - {DateTime.MinValue:dd/MM/yyyy} - {DateTime.MaxValue:dd/MM/yyyy} UC: 1 UA: 2 ObjectGuid: {entity.ObjectGuid}";

            var dto = _mapper.Map(model, entity);

            Assert.Equal(EXPECTED, $"{dto.Id} - {dto.Nome} - {dto.CodigoIbge} - {DateTime.MinValue:dd/MM/yyyy} - {DateTime.MaxValue:dd/MM/yyyy} UC: 1 UA: 2 ObjectGuid: {dto.ObjectGuid}");

        }

    }
}
