using FluentNHibernate.Mapping;
using SW_PortalProprietario.Domain.Entities.Core.Sistema;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.Sistema
{
    public class SagaExecutionMap : ClassMap<SagaExecution>
    {
        public SagaExecutionMap()
        {
            Id(x => x.Id).GeneratedBy.Native("SagaExecution_");

            // SagaId é string (GUID como string), não System.Guid
            Map(b => b.SagaId).Length(100).Not.Nullable().Index("IX_SagaExecution_SagaId");
            Map(b => b.OperationType).Length(200).Not.Nullable();
            Map(b => b.Status).Length(50).Not.Nullable();
            Map(b => b.InputData).CustomType("StringClob").CustomSqlType("Text").Nullable();
            Map(b => b.OutputData).CustomType("StringClob").CustomSqlType("Text").Nullable();
            Map(b => b.ErrorMessage).Length(2000).Nullable();
            Map(b => b.DataHoraInicio).Not.Nullable();
            Map(b => b.DataHoraConclusao).Nullable();
            Map(b => b.DuracaoMs).Nullable();
            Map(b => b.UsuarioId).Nullable();
            Map(b => b.Endpoint).Length(500).Nullable();
            Map(b => b.ClientIp).Length(50).Nullable();
            Map(b => b.Metadata).CustomType("StringClob").CustomSqlType("Text").Nullable();

            HasMany(x => x.Steps)
                .KeyColumn("SagaExecutionId")
                .Cascade.AllDeleteOrphan()
                .Inverse()
                .LazyLoad();

            Table("SagaExecution");
        }
    }
}
