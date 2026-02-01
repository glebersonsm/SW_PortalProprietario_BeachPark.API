using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class TelEndPessMap : ClassMap<TelEndPess>
    {
        public TelEndPessMap()
        {
            Id(x => x.IdTelefone).GeneratedBy.Sequence("SEQTELENDPESS");

            Map(p => p.IdEndereco);

            Map(b => b.Ddi);

            Map(b => b.Ddd);
            Map(b => b.Numero);
            Map(b => b.Tipo);

            Map(b => b.TrgDtInclusao);
            Map(b => b.TrgUserInclusao);
            Map(b => b.TrgDtAlteracao);
            Map(b => b.TrgUserAlteracao);

            Table("TelEndPess");
        }
    }
}
