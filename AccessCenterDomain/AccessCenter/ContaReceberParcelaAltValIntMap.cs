using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class ContaReceberParcelaAltValIntMap : ClassMap<ContaReceberParcelaAltValInt>
    {
        public ContaReceberParcelaAltValIntMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("CONTARECEBERPARCELAALTVALINT_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(p => p.ContaReceberParcelaBaixa);
            Map(b => b.ContaReceberParcelaAltVal);
            Map(b => b.Valor);

            Table("ContaReceberParcelaAltValInt");
        }
    }
}
