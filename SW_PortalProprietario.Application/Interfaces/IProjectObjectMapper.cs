namespace SW_PortalProprietario.Application.Interfaces
{
    public interface IProjectObjectMapper
    {
        TDestination Map<TDestination>(object? source);
        TDestination Map<TSource, TDestination>(TSource source, TDestination destination);
    }
}
