using AutoMapper;
using SW_PortalProprietario.Application.Interfaces;

namespace SW_PortalProprietario.Infra.Ioc.Mapper
{
    public class ProjectObjectMapper : IProjectObjectMapper
    {
        private readonly IMapper _mapper;

        public ProjectObjectMapper(IMapper mapper)
        {
            _mapper = mapper;

        }

        public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
        {
            return _mapper.Map(source, destination);
        }

        public TDestination Map<TDestination>(object? source)
        {
            return _mapper.Map<TDestination>(source);
        }
    }
}
