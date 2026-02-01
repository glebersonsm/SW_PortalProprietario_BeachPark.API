using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class AtividadeProjetoMap : ClassMap<AtividadeProjeto>
    {
        public AtividadeProjetoMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("ATIVIDADEPROJETO_SEQUENCE");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(b => b.GrupoEmpresa);
            Map(b => b.Codigo);
            Map(b => b.Nome);
            Map(p => p.NomePesquisa);

            Table("AtividadeProjeto");
        }
    }
}
