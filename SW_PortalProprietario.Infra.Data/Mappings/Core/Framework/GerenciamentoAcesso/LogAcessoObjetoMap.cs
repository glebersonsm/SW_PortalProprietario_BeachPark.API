using FluentNHibernate.Mapping;
using SW_PortalProprietario.Domain.Entities.Core.Framework.GerenciamentoAcesso;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.Framework.GerenciamentoAcesso
{
    public class LogAcessoObjetoMap : ClassMap<LogAcessoObjeto>
    {
        public LogAcessoObjetoMap()
        {
            Id(x => x.Id).GeneratedBy.Native("LogAcessoObjeto_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.DataHoraAlteracao).Nullable();
            Map(p => p.UsuarioAlteracao).Nullable();
            Map(b => b.ObjectType).Length(500);
            Map(b => b.ObjectOperationGuid).Length(200);
            Map(b => b.ObjectId);
            Map(b => b.DataHoraOperacao);
            Map(b => b.UsuarioOperacao);
            Map(b => b.TipoOperacao);

            Table("LogAcessoObjeto");
        }
    }
}
