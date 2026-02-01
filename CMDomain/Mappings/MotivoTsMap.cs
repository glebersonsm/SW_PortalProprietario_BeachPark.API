using CMDomain.Entities;
using FluentNHibernate.Mapping;

namespace CMDomain.Mappings
{
    public class MotivoTsMap : ClassMap<MotivoTs>
    {
        public MotivoTsMap()
        {
            Id(x => x.IdMotivoTs)
                .GeneratedBy.Assigned();

            Map(p => p.CodReduzido);

            Map(b => b.Descricao);

            Map(b => b.Aplicacao);

            Map(p => p.FlgAtivo);

            Map(b => b.FlgLancFuturo);

            Map(b => b.FlgContaUtilizacao);

            Map(b => b.IdTipoDcCred);
            Map(b => b.IdHotel);
            Map(b => b.IdTipoDcDeb);
            Map(b => b.FlgPermlancCredito);
            Map(b => b.IdDepartamento);
            Map(b => b.Observacao);
            Map(b => b.FlgGeraComissao);
            Map(b => b.FlgBloqueiaComissao);
            Map(b => b.FlgNegociacao);
            Map(b => b.IdTipoIndice);
            Map(b => b.FlgEncerraAutoOcorrencia);

            Map(b => b.TrgDtInclusao);
            Map(b => b.TrgUserInclusao);

            Table("MotivoTs");
        }
    }
}
