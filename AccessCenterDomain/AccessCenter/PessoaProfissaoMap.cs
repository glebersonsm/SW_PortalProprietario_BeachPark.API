using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class PessoaProfissaoMap : ClassMap<PessoaProfissao>
    {
        public PessoaProfissaoMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("PESSOAPROFISSAO_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(p => p.Profissao);
            Map(b => b.Principal);
            Map(b => b.Pessoa);

            Table("PessoaProfissao");
        }
    }
}
