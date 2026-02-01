using AccessCenterDomain.AccessCenter.Fractional;
using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class FrHotelTipoUhMap : ClassMap<FrHotelTipoUh>
    {
        public FrHotelTipoUhMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("FRHOTELTIPOUH_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(b => b.Filial);
            Map(b => b.Codigo);
            Map(b => b.Nome);
            Map(b => b.NomePesquisa);
            Map(b => b.IdIntegracao);
            Map(b => b.Status);
            Map(b => b.FrHotel);
            Map(b => b.Capacidade);

            Table("FrHotelTipoUh");
        }
    }
}
