using NHibernate;

namespace SW_PortalProprietario.Application.Interfaces
{
    public interface ISwPortalSessionFactory
    {
        IStatelessSession OpenStatelessSession();
    }
}
