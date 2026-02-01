using FluentNHibernate.Mapping;
using SW_PortalProprietario.Domain.Entities.Core.Geral;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.Geral
{
    public class HistoricoDocumentoMap : ClassMap<HistoricoDocumento>
    {
        public HistoricoDocumentoMap()
        {
            Id(x => x.Id).GeneratedBy.Native("HistoricoDocumento_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.DataHoraAlteracao).Nullable();
            Map(p => p.UsuarioAlteracao).Nullable();
            Map(p => p.UsuarioRemocao).Nullable();
            Map(p => p.DataHoraRemocao).Nullable();

            Map(p => p.Path);
            References(b => b.Documento, "Documento");
            Map(b => b.Acao).Length(100);
            Table("HistoricoDocumento");
        }
    }
}

