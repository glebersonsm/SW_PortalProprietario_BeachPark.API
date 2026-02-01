using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class CentroCustoFilialMap : ClassMap<CentroCustoFilial>
    {
        public CentroCustoFilialMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("CENTROCUSTOFILIAL_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);


            Map(b => b.Filial);
            Map(b => b.CentroCusto);
            Map(p => p.StatusLancamento);
            Map(p => p.StatusConsulta);

            Table("CentroCustoFilial");
        }
    }
}
