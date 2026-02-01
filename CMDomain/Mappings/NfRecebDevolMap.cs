using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class NfRecebDevolMap : ClassMap<NfRecebDevol>
    {
        public NfRecebDevolMap()
        {
            Id(x => x.IdNfRecebDevol).GeneratedBy.Assigned();

            Map(p => p.IdHotel);
            Map(p => p.IdForCli);
            Map(p => p.CodDocumento);
            Map(p => p.NumNf);
            Map(p => p.IdRequestia);
            Map(p => p.Processado);
            Map(p => p.IdArquivo);

            Table("NfRecebDevol");
        }
    }
}
