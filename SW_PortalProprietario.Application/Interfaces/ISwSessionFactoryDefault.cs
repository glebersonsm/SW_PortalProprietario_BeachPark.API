using NHibernate;
namespace SW_PortalProprietario.Application.Interfaces
{
    public interface ISwSessionFactoryDefault
    {
        IStatelessSession OpenStatelessSession();
    }
}
