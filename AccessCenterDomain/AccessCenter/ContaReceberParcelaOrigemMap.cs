using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class ContaReceberParcelaOrigemMap : ClassMap<ContaReceberParcelaOrigem>
    {
        public ContaReceberParcelaOrigemMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("CONTARECEBERPARCELAORIGEM_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(p => p.ContaReceberParcela);
            Map(b => b.ContaReceberParcelaDestino);
            Map(b => b.Tipo);
            Map(b => b.Valor);
            Map(b => b.ContaReceberParcelaAltVal);

            Table("ContaReceberParcelaOrigem");
        }
    }
}
