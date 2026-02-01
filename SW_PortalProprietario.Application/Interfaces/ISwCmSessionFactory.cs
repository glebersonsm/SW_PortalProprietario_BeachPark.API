using NHibernate;

namespace SW_PortalProprietario.Application.Interfaces
{
    public interface ISwCmSessionFactory
    {
        IStatelessSession OpenStatelessSession();
    }
}
