using NHibernate;
namespace SW_PortalProprietario.Application.Interfaces
{
    public interface ISwSessionFactoryHosted
    {
        IStatelessSession OpenStatelessSession();
    }
}
