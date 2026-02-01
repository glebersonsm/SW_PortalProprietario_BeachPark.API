using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class TipoAlmoxarifadoMap : ClassMap<TipoAlmoxarifado>
    {
        public TipoAlmoxarifadoMap()
        {
            Id(x => x.Id).GeneratedBy.Sequence("TIPOBAIXA_SEQUENCE");
            Map(b => b.Tag);
            Map(b => b.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.UsuarioAlteracao);
            Map(b => b.DataHoraAlteracao);

            Map(b => b.GrupoEmpresa);
            Map(b => b.Codigo);
            Map(b => b.Nome);
            Map(b => b.NomePesquisa);
            Map(b => b.Status);

            Table("TipoAlmoxarifado");
        }
    }
}
