using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class CotaProprietarioMap : ClassMap<CotaProprietario>
    {
        public CotaProprietarioMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("COTAPROPRIETARIO_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(b => b.Cota);
            Map(b => b.Proprietario);
            Map(b => b.Procurador);
            Map(b => b.DataAquisicao);

            Table("CotaProprietario");
        }
    }
}
