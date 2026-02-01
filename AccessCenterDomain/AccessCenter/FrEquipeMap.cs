using AccessCenterDomain.AccessCenter.Fractional;
using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class FrEquipeMap : ClassMap<FrEquipe>
    {
        public FrEquipeMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("FREQUIPE_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(b => b.Codigo);
            Map(b => b.Empresa);
            Map(b => b.Nome);

            Table("FrEquipe");
        }
    }
}
