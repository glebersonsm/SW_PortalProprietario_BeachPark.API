using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class LfItemXEmpresaMap : ClassMap<LfItemXEmpresa>
    {
        public LfItemXEmpresaMap()
        {
            CompositeId()
                .KeyProperty(x => x.CodItem)
                .KeyProperty(x => x.IdPessoa);

            Map(p => p.CodStIpi);
            Map(p => p.CodStCofins);
            Map(p => p.CodStPis);
            Map(p => p.CodStb);
            Map(p => p.FlgTipo);
            Map(p => p.FlgRegApuPisCof);
            Map(p => p.FlgRegra);
            Map(p => p.FlgIndicadorProp);
            Map(p => p.ContaContDebito);
            Map(p => p.ContaContCredito);
            Map(p => p.CentroCustoCredito);
            Map(p => p.CentroCustoDebito);
            Map(p => p.TrgDtInclusao);
            Map(p => p.TrgUserInclusao);

            Table("LfItemXEmpresa");
        }
    }
}
