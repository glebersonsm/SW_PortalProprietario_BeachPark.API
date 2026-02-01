using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class ProfissaoMap : ClassMap<Profissao>
    {
        public ProfissaoMap()
        {
            Id(x => x.Id).GeneratedBy.Sequence("PROFISSAO_SEQUENCE");
            Map(b => b.Tag);
            Map(b => b.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.UsuarioAlteracao);
            Map(b => b.DataHoraAlteracao);

            Map(b => b.Codigo);
            Map(b => b.Nome);
            Map(b => b.NomePesquisa);
            Map(b => b.Status);
            Map(b => b.QualificacaoAutomaticaQua);

            Table("Profissao");
        }
    }
}
