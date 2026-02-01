using AccessCenterDomain.AccessCenter.Fractional;
using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class FrDpnuMap : ClassMap<FrDpnu>
    {
        public FrDpnuMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("FRDPNU_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(b => b.Filial);
            Map(b => b.Empresa);
            Map(b => b.CodigoAgrupamentoLancamento);
            Map(b => b.FrAtendimentoVenda);
            Map(b => b.DataLancamento);

            Table("FrDpnu");
        }
    }
}
