using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class PessoaTelefoneMap : ClassMap<PessoaTelefone>
    {
        public PessoaTelefoneMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("PESSOATELEFONE_SEQUENCE");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(p => p.Numero);
            Map(b => b.Preferencial);
            Map(b => b.Estrangeiro);
            Map(b => b.RecebeSms);
            Map(b => b.Pais);
            Map(b => b.TipoTelefone);
            Map(b => b.Pessoa);

            Table("PessoaTelefone");
        }
    }
}
