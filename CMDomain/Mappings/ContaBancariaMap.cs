using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class ContaBancariaMap : ClassMap<ContaBancaria>
    {
        public ContaBancariaMap()
        {
            Id(x => x.IdCBancaria).GeneratedBy.Sequence("SEQCONTABANCARIA");

            Map(p => p.IdPessoa);
            Map(p => p.ContaCorrente);
            Map(p => p.IdAgencia);
            Map(p => p.FlgContaPref);
            Map(p => p.TipoConta);
            Map(p => p.FlgInativa);
            Map(p => p.TrgUserInclusao);
            Map(p => p.TrgDtInclusao);

            Table("ContaBancaria");
        }
    }
}
