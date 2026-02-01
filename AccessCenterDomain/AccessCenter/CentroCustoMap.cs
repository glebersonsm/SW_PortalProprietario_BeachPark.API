using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class CentroCustoMap : ClassMap<CentroCusto>
    {
        public CentroCustoMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("CENTROCUSTO_SEQUENCE");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);


            Map(b => b.GrupoEmpresa);
            Map(b => b.Codigo);
            Map(b => b.Nome);
            Map(b => b.NomePesquisa);
            Map(p => p.StatusLancamento);
            Map(p => p.StatusConsulta);
            Map(p => p.AnaliticoSintetico);
            Map(p => p.RestringeFilial);
            Map(p => p.EmailResponsavel);
            Map(p => p.Parent);
            Map(p => p.CentroResultado);
            Map(p => p.RestringeUsuario);

            Table("CentroCusto");
        }
    }
}
