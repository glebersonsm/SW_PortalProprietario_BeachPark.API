using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class HotelMap : ClassMap<Hotel>
    {
        public HotelMap()
        {
            Id(x => x.IdHotel).GeneratedBy.Assigned();

            Map(p => p.IdEmpresa).Nullable();
            Map(p => p.IdPessoa);
            Map(p => p.IdUnidNegoc).Nullable();
            Map(p => p.IdRegiaoHotel).Nullable();
            Map(p => p.IdEmpCondominio).Nullable();
            Map(p => p.IdRedeHotel).Nullable();
            Map(p => p.Ativo).Nullable();
            Map(p => p.FlgUsaNoSistema).Nullable();
            Map(p => p.FlgOutrosHoteis).Nullable();
            Map(p => p.NomeEmpreendimento).Nullable();
            Map(p => p.FlgTimeSharing).Nullable();
            Map(p => p.FlgSpa).Nullable();

            Table("Hotel");
        }
    }
}
