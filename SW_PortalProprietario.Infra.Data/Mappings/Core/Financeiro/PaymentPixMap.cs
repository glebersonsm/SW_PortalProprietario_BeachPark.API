using FluentNHibernate.Mapping;
using SW_PortalProprietario.Domain.Entities.Core.Financeiro;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.Financeiro
{
    public class PaymentPixMap : ClassMap<PaymentPix>
    {
        public PaymentPixMap()
        {
            Id(x => x.Id).GeneratedBy.Native("PaymentPix_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.DataHoraAlteracao).Nullable();
            Map(p => p.UsuarioAlteracao).Nullable();
            Map(b => b.CompanyId);
            Map(b => b.Valor);
            Map(b => b.Acquirer);
            References(b => b.Pessoa, "Pessoa");
            Map(b => b.Status);
            Map(b => b.PaymentId);
            Map(b => b.Pdf).Length(2000);
            Map(b => b.Payment_Id);
            Map(b => b.QrCode).Length(2000);
            Map(b => b.TransactionId).Length(2000);
            Map(b => b.Url).Length(2000);
            Map(b => b.DadosEnviados).CustomType("StringClob").CustomSqlType("Text");
            Map(b => b.Retorno).CustomType("StringClob").CustomSqlType("Text");
            Map(b => b.ValidoAte);
            Map(b => b.AgrupamentoBaixaLegadoId);
            Map(b => b.EmpresaLegadoId);
            Schema("portalohana");
            Table("PaymentPix");
        }
    }
}
