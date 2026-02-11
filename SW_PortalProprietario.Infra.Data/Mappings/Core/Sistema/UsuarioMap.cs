using FluentNHibernate.Mapping;
using NHibernate.Type;
using SW_PortalProprietario.Domain.Entities.Core.Sistema;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.Sistema
{
    public class UsuarioMap : ClassMap<Usuario>
    {
        public UsuarioMap()
        {
            Id(x => x.Id).GeneratedBy.Native("Usuario_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);

            Map(b => b.DataHoraCriacao);

            Map(b => b.DataHoraAlteracao)
                .Nullable();

            Map(p => p.UsuarioAlteracao)
                .Nullable();

            Map(p => p.PasswordHash)
                .Not
                .Nullable();

            References(p => p.Pessoa, "Pessoa").UniqueKey("UK_UsuarioPessoa");


            Map(b => b.Login).Length(100).Unique();

            Map(b => b.Status).CustomType<EnumType<EnumStatus>>();

            Map(b => b.Administrador).CustomType<EnumType<EnumSimNao>>();
            Map(b => b.GestorFinanceiro).CustomType<EnumType<EnumSimNao>>();
            Map(b => b.GestorReservasAgendamentos).CustomType<EnumType<EnumSimNao>>();
            Map(b => b.UsuarioAdministrativo).CustomType<EnumType<EnumSimNao>>();
            Map(b => b.ProviderChaveUsuario);
            Map(b => b.TokenResult).CustomType("StringClob").CustomSqlType("Text");
            Map(b => b.Removido).CustomType<EnumType<EnumSimNao>>();
            Map(b => b.DataHoraRemocao);
            Map(b => b.LoginPms);
            Map(b => b.LoginSistemaVenda);
            Map(b => b.AvatarBase64).CustomType("StringClob").CustomSqlType("Text").Nullable();
            Map(b => b.MenuPermissions).CustomType("StringClob").CustomSqlType("Text").Nullable();

            Table("Usuario");
        }
    }
}
