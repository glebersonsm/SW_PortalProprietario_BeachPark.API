namespace SW_PortalProprietario.Application.Services.Providers.Interfaces
{
    public interface ITokenBodyService
    {
        Dictionary<string, object> GetBodyToken(string token);
    }
}
