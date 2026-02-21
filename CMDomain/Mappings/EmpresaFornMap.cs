using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class EmpresaFornMap : ClassMap<EmpresaForn>
    {
        public EmpresaFornMap()
        {
            CompositeId()
            .KeyProperty(x => x.IdForCli)
            .KeyProperty(x => x.IdPessoa);

            Map(p => p.IdEmpresa);
            Map(p => p.Plano);
            Map(p => p.ContaCadiantamento);
            Map(p => p.ContaCDespesa);
            Map(p => p.ContaCForn);
            Map(p => p.CodSubConta);
            Map(p => p.CodCorresp);
            Map(p => p.FlgStatus);
            Map(p => p.FlgNaoContabLanc);
            Map(p => p.FlgContBaixDesemb);
            Map(p => p.IndOpCcp);
            Map(p => p.TrgDtInclusao);
            Map(p => p.TrgUserInclusao);

            Schema("cm");
            Table("EmpresaForn");
        }
    }
}
