using AccessCenterDomain.AccessCenter.Fractional;
using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class FrSalaMap : ClassMap<FrSala>
    {
        public FrSalaMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("FrSala_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(b => b.Filial);
            Map(b => b.Codigo);
            Map(b => b.Nome);
            Map(b => b.NomePesquisa);
            Map(b => b.Status);
            Map(b => b.Empresa);
            Map(b => b.GrupoEmpresa);

            Table("FrSala");
        }
    }
}
