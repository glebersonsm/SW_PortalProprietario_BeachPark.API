using FluentNHibernate.Mapping;
using SW_PortalProprietario.Domain.Entities.Core.Financeiro;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.Financeiro
{
    public class PaymentCardTokenizedMap : ClassMap<PaymentCardTokenized>
    {
        public PaymentCardTokenizedMap()
        {
            Id(x => x.Id).GeneratedBy.Native("PaymentCardTokenized_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.DataHoraAlteracao).Nullable();
            Map(p => p.UsuarioAlteracao).Nullable();
            Map(b => b.Valor);
            Map(b => b.PaymentId);
            References(b => b.CardTokenized, "CardTokenized");
            Map(b => b.Status);
            Map(b => b.Nsu);
            Map(b => b.CodigoAutorizacao);
            Map(b => b.Adquirente);
            Map(b => b.AdquirentePaymentId);
            Map(b => b.TransactionId).Length(2000);
            Map(b => b.Url);
            Map(b => b.CompanyId);
            Map(b => b.ParcelasSincronizadas);
            Map(b => b.EmpresaLegadoId);
            Map(b => b.ResultadoSincronizacaoParcelas).CustomType("StringClob").CustomSqlType("Text");
            Map(b => b.DadosEnviados).CustomType("StringClob").CustomSqlType("Text");
            Map(b => b.Retorno).CustomType("StringClob").CustomSqlType("Text");
            Schema("portalohana");
            Table("PaymentCardTokenized");
        }
    }
}
