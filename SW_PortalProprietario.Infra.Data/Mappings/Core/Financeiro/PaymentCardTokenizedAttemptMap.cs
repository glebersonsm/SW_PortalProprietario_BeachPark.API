using FluentNHibernate.Mapping;
using SW_PortalProprietario.Domain.Entities.Core.Financeiro;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.Financeiro
{
    public class PaymentCardTokenizedAttemptMap : ClassMap<PaymentCardTokenizedAttempt>
    {
        public PaymentCardTokenizedAttemptMap()
        {
            Id(x => x.Id).GeneratedBy.Native("PaymentCardTokenizedAttempt_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.DataHoraAlteracao).Nullable();
            Map(p => p.UsuarioAlteracao).Nullable();
            Map(b => b.Valor);
            Map(b => b.DadosEnviados).CustomType("StringClob").CustomSqlType("Text");
            Map(b => b.Retorno).CustomType("StringClob").CustomSqlType("Text");
            Map(b => b.RetornoAmigavel);
            References(b => b.CardTokenized, "CardTokenized");
            Map(b => b.EmpresaLegadoId);

            Table("PaymentCardTokenizedAttempt");
        }
    }
}
