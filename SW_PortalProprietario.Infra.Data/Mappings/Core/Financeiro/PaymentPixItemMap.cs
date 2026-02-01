using FluentNHibernate.Mapping;
using SW_PortalProprietario.Domain.Entities.Core.Financeiro;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.Financeiro
{
    public class PaymentPixItemMap : ClassMap<PaymentPixItem>
    {
        public PaymentPixItemMap()
        {
            Id(x => x.Id).GeneratedBy.Native("PaymentPixItem_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.DataHoraAlteracao).Nullable();
            Map(p => p.UsuarioAlteracao).Nullable();
            Map(b => b.Valor);
            Map(b => b.ItemId);
            Map(b => b.ValorNaTransacao);
            Map(b => b.Vencimento);
            Map(b => b.DescricaoDoItem);
            References(b => b.PaymentPix, "PaymentPix");

            Table("PaymentPixItem");
        }
    }
}
