using FluentNHibernate.Mapping;

namespace AccessCenterDomain.AccessCenter
{
    public class ContaReceberParcelaBaixaIntMap : ClassMap<ContaReceberParcelaBaixaInt>
    {
        public ContaReceberParcelaBaixaIntMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("CONTARECEBERPARCELABAIXAINT_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(p => p.ContaReceberParcelaBaixa);
            Map(b => b.ContaReceberParcelaOrigem);
            Map(b => b.Valor);

            Table("ContaReceberParcelaBaixaInt");
        }
    }
}
