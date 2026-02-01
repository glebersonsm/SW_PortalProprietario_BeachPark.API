using FluentNHibernate.Mapping;
using SW_PortalProprietario.Domain.Entities.Core.Geral;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.Geral
{
    public class RegraPaxFreeConfiguracaoMap : ClassMap<RegraPaxFreeConfiguracao>
    {
        public RegraPaxFreeConfiguracaoMap()
        {
            Id(x => x.Id).GeneratedBy.Native("RegraPaxFreeConfiguracao_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.DataHoraAlteracao).Nullable();
            Map(p => p.UsuarioAlteracao).Nullable();
            Map(p => p.UsuarioRemocao).Nullable();
            Map(p => p.DataHoraRemocao).Nullable();

            References(b => b.RegraPaxFree, "RegraPaxFree");
            Map(b => b.QuantidadeAdultos).Nullable();
            Map(b => b.QuantidadePessoasFree).Nullable();
            Map(b => b.IdadeMaximaAnos).Nullable();
            Map(b => b.TipoOperadorIdade).Length(10).Nullable();
            Map(b => b.TipoDataReferencia).Length(20).Nullable();

            Table("RegraPaxFreeConfiguracao");
        }
    }
}

