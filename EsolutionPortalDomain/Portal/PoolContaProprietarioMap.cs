using FluentNHibernate.Mapping;


namespace EsolutionPortalDomain.Portal
{
    public class PoolCotaProprietarioMap : ClassMap<PoolCotaProprietario>
    {
        public PoolCotaProprietarioMap()
        {
            Id(x => x.Id)
            .GeneratedBy.SequenceIdentity();

            Map(p => p.Pool);
            Map(b => b.CotaProprietario);
            Map(b => b.DataInclusao);
            Map(b => b.DataSaida);
            Map(b => b.MinimoSemanas);
            Map(b => b.MaximoSemanas);
            Map(b => b.UsuarioInclusao);
            Map(b => b.DataHoraInclusao);
            Map(b => b.UsuarioSaida);
            Map(b => b.DataHoraSaida);
            Map(b => b.UsuarioExclusao);
            Map(b => b.DataHoraExclusao);

            Table("PoolCotaProprietario");
        }
    }
}
