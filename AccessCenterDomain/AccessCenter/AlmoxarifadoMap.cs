using FluentNHibernate.Mapping;

namespace AccessCenterDomain.AccessCenter
{
    public class AlmoxarifadoMap : ClassMap<Almoxarifado>
    {
        public AlmoxarifadoMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("ALMOXARIFADO_SEQUENCE");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(b => b.GrupoEmpresa);
            Map(b => b.Codigo);
            Map(b => b.Nome);
            Map(b => b.Ativo);
            References(b => b.Filial, "Filial");
            References(b => b.Empresa, "Empresa");
            Map(b => b.NomeAbreviado);
            References(b => b.TipoAlmoxarifado, "TipoAlmoxarifado");

            Table("Almoxarifado");
        }
    }
}
