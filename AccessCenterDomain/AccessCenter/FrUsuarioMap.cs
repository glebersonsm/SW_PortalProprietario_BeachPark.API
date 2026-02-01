using AccessCenterDomain.AccessCenter.Fractional;
using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class FrUsuarioMap : ClassMap<FrUsuario>
    {
        public FrUsuarioMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("FRUSUARIO_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(b => b.GrupoEmpresa);
            Map(b => b.Empresa);
            Map(b => b.Nome);
            Map(b => b.NomePesquisa);
            Map(b => b.StatusLancamento);
            Map(b => b.StatusConsulta);
            Map(b => b.PossuiAjudaCusto);
            Map(b => b.UsuarioSistema);

            Table("FrUsuario");
        }
    }
}
