using AccessCenterDomain.AccessCenter.Fractional;
using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class FrHotelMap : ClassMap<FrHotel>
    {
        public FrHotelMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("FRHOTEL_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(b => b.Filial);
            Map(b => b.Codigo);
            Map(b => b.Nome);
            Map(b => b.NomePesquisa);
            Map(b => b.Integracao);
            Map(b => b.IdIntegracao);
            Map(b => b.PermitirReservaNorDifSetDia);
            Map(b => b.CobraTaxaUtilizacao);
            Map(b => b.IdadeCrianca1Inicio);
            Map(b => b.IdadeCrianca1Fim);
            Map(b => b.IdadeCrianca2Inicio);
            Map(b => b.IdadeCrianca2Fim);

            Table("FrHotel");
        }
    }
}
