using NHibernate;
using SW_PortalProprietario.Application.Interfaces;

namespace SW_PortalProprietario.Infra.Data.SwNHibernate
{
    public class SwAccessCenterSessionFactory : ISwAccessCenterSessionFactory
    {
        public ISessionFactory? SessionFactory { get; set; }

        public IStatelessSession OpenStatelessSession()
        {
            return SessionFactory!.OpenStatelessSession();
        }
    }
}
