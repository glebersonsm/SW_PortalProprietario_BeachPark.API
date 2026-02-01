using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class CidadeMap : ClassMap<Cidade>
    {
        public CidadeMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("CIDADE_SEQUENCE");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(b => b.Nome);
            Map(b => b.NomePesquisa);
            Map(p => p.Estado);
            Map(p => p.CodigoIbge);

            Table("Cidade");
        }
    }
}
