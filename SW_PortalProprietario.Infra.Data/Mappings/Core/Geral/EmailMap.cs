using FluentNHibernate.Mapping;
using SW_PortalProprietario.Domain.Entities.Core.Geral;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.Geral
{
    public class EmailMap : ClassMap<Email>
    {
        public EmailMap()
        {
            Id(x => x.Id).GeneratedBy.Native("Email_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.DataHoraAlteracao).Nullable();
            Map(p => p.UsuarioAlteracao).Nullable();
            Map(b => b.Assunto).Length(2000);
            Map(b => b.Destinatario).Length(2000);
            Map(b => b.ConteudoEmail).CustomType("StringClob").CustomSqlType("Text");
            Map(b => b.Enviado).CustomType<EnumSimNao>();
            Map(b => b.NaFila).CustomType<EnumSimNao>();
            Map(p => p.DataHoraEnvio);
            Map(p => p.DataHoraPrimeiraAbertura).Nullable();
            Map(b => b.ErroEnvio).CustomType("StringClob").CustomSqlType("Text");
            References(x => x.Empresa, "Empresa");

            HasMany(x => x.Anexos)
                .KeyColumn("EmailId")
                .Cascade.AllDeleteOrphan()
                .Inverse()
                .LazyLoad();

            Table("Email");
        }

    }
}
