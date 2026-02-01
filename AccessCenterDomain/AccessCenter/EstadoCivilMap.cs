using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class EstadoCivilMap : ClassMap<EstadoCivil>
    {
        public EstadoCivilMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("ESTADOCIVIL_SEQUENCE");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(b => b.Codigo);
            Map(b => b.Nome);
            Map(b => b.NomePesquisa);
            //Map(p => p.Estado);
            Map(p => p.Categoria);
            Map(p => p.PossuiConjuge);

            Table("EstadoCivil");
        }
    }
}
