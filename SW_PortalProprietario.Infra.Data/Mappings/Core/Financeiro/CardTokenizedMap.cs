using FluentNHibernate.Mapping;
using NHibernate.Type;
using SW_PortalProprietario.Domain.Entities.Core.Financeiro;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.Financeiro
{
    public class CardTokenizedMap : ClassMap<CardTokenized>
    {
        public CardTokenizedMap()
        {
            Id(x => x.Id).GeneratedBy.Native("CardTokenized_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.DataHoraAlteracao).Nullable();
            Map(p => p.UsuarioAlteracao).Nullable();
            Map(b => b.CardHolder).Length(200);
            Map(b => b.Brand);
            Map(b => b.CardNumber);
            Map(b => b.Cvv);
            Map(b => b.DueDate);
            Map(b => b.Token);
            Map(b => b.Token2);
            Map(b => b.Status);
            Map(b => b.CompanyId);
            Map(b => b.CompanyToken);
            Map(b => b.Acquirer);
            Map(b => b.ClienteId);
            Map(b => b.Hash);
            Map(b => b.EmpresaLegadoId);
            Map(b => b.Visivel).CustomType<EnumType<EnumSimNao>>();
            References(b => b.Pessoa, "Pessoa");

            Table("CardTokenized");
        }
    }
}
