using NHibernate;

namespace SW_PortalProprietario.Application.Interfaces
{
    public interface ISwAccessCenterSessionFactory
    {
        IStatelessSession OpenStatelessSession();
    }
}
