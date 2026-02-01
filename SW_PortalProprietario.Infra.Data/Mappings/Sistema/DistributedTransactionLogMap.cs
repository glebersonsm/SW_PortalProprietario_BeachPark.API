using FluentNHibernate.Mapping;
using SW_PortalProprietario.Domain.Entities.Core.Sistema;

namespace SW_PortalProprietario.Infra.Data.Mappings.Sistema
{
    /// <summary>
    /// Mapeamento NHibernate para DistributedTransactionLog
    /// </summary>
    public class DistributedTransactionLogMap : ClassMap<DistributedTransactionLog>
    {
        public DistributedTransactionLogMap()
        {
            Table("DistributedTransactionLog");
            
            Id(x => x.Id)
                .Column("Id")
                .GeneratedBy.Identity();

            Map(x => x.OperationId)
                .Column("OperationId")
                .Length(50)
                .Not.Nullable();

            Map(x => x.OperationType)
                .Column("OperationType")
                .Length(100)
                .Not.Nullable();

            Map(x => x.StepName)
                .Column("StepName")
                .Length(100)
                .Not.Nullable();

            Map(x => x.StepOrder)
                .Column("StepOrder")
                .Not.Nullable();

            Map(x => x.Status)
                .Column("Status")
                .Length(20)
                .Not.Nullable();

            Map(x => x.Payload)
                .Column("Payload")
                .Length(4000)
                .Nullable();

            Map(x => x.ErrorMessage)
                .Column("ErrorMessage")
                .Length(2000)
                .Nullable();

            Map(x => x.DataHoraCriacao)
                .Column("DataHoraCriacao")
                .Not.Nullable();

            Map(x => x.DataHoraCompensacao)
                .Column("DataHoraCompensacao")
                .Nullable();

            Map(x => x.UsuarioCriacao)
                .Column("UsuarioCriacao")
                .Nullable();

            Map(x => x.AdditionalData)
                .Column("AdditionalData")
                .Length(4000)
                .Nullable();
        }
    }
}
