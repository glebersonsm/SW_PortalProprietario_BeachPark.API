using FluentNHibernate.Mapping;
using SW_PortalProprietario.Domain.Entities.Core.Sistema;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.Sistema
{
    public class SagaStepMap : ClassMap<SagaStep>
    {
        public SagaStepMap()
        {
            Id(x => x.Id).GeneratedBy.Native("SagaStep_");

            Map(b => b.StepName).Length(200).Not.Nullable();
            Map(b => b.StepOrder).Not.Nullable();
            Map(b => b.Status).Length(50).Not.Nullable();
            Map(b => b.InputData).CustomType("StringClob").CustomSqlType("Text").Nullable();
            Map(b => b.OutputData).CustomType("StringClob").CustomSqlType("Text").Nullable();
            Map(b => b.ErrorMessage).Length(2000).Nullable();
            Map(b => b.StackTrace).CustomType("StringClob").CustomSqlType("Text").Nullable();
            Map(b => b.DataHoraInicio).Nullable();
            Map(b => b.DataHoraConclusao).Nullable();
            Map(b => b.DuracaoMs).Nullable();
            Map(b => b.DataHoraInicioCompensacao).Nullable();
            Map(b => b.DataHoraConclusaoCompensacao).Nullable();
            Map(b => b.DuracaoCompensacaoMs).Nullable();
            Map(b => b.Tentativas).Not.Nullable();
            Map(b => b.TentativasCompensacao).Not.Nullable();
            Map(b => b.PodeSerCompensado).Not.Nullable();
            Map(b => b.Metadata).CustomType("StringClob").CustomSqlType("Text").Nullable();

            References(x => x.SagaExecution, "SagaExecution").Not.Nullable();

            Schema("portalohana");
            Table("SagaStep");
        }
    }
}
