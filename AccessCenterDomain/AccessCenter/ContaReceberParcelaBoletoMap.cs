using FluentNHibernate.Mapping;

namespace AccessCenterDomain.AccessCenter
{
    public class ContaReceberParcelaBoletoMap : ClassMap<ContaReceberParcelaBoleto>
    {
        public ContaReceberParcelaBoletoMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("CONTARECEBERPARCELABOLETO_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(p => p.ContaReceberBoleto);
            Map(b => b.ContaReceberParcela);

            Table("ContaReceberParcelaBoleto");
        }
    }
}
