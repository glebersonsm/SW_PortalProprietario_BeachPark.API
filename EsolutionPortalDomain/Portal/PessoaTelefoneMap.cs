using FluentNHibernate.Mapping;


namespace EsolutionPortalDomain.Portal
{
    public class PessoaTelefoneMap : ClassMap<PessoaTelefone>
    {
        public PessoaTelefoneMap()
        {
            Id(x => x.Id)
            .GeneratedBy.SequenceIdentity();

            Map(p => p.Pessoa);
            Map(b => b.Tipo);
            Map(b => b.Numero);
            Map(b => b.Preferencial);

            Table("PessoaTelefone");
        }
    }
}
