using FluentNHibernate.Mapping;
using SW_PortalProprietario.Domain.Entities.Core.DadosPessoa;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.DadosPessoa
{
    public class PessoaSistemaXProviderMap : ClassMap<PessoaSistemaXProvider>
    {
        public PessoaSistemaXProviderMap()
        {
            Id(x => x.Id).GeneratedBy.Native("PessoaSistemaXProvider_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.DataHoraAlteracao).Nullable();
            Map(p => p.UsuarioAlteracao).Nullable();
            Map(b => b.PessoaProvider).Length(100);
            Map(b => b.PessoaSistema).Length(100);
            Map(b => b.NomeProvider);
            Map(b => b.TokenResult).CustomType("StringClob").CustomSqlType("Text");

            Table("PessoaSistemaXProvider");
        }
    }
}
