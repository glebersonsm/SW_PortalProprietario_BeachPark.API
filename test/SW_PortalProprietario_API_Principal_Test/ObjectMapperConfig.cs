using AutoMapper;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Infra.Data.ModelProfiles;

namespace SW_PortalProprietario.Test
{
    public static class ObjectMapperConfig
    {
        private static IProjectObjectMapper? _mapper;
        public static IProjectObjectMapper Initialize()
        {
            if (_mapper != null) return _mapper;
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AllowNullCollections = true;
                cfg.AddProfile<AutoMapperConfigurationProfiles>();
            });

            IMapper mapper = configuration.CreateMapper();
            _mapper = new Infra.Ioc.Mapper.ProjectObjectMapper(mapper);
            return _mapper;

        }
    }
}
