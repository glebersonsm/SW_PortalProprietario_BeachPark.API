using AccessCenterDomain.AccessCenter.Fractional;
using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class FrUsuarioFrFuncaoMap : ClassMap<FrUsuarioFrFuncao>
    {
        public FrUsuarioFrFuncaoMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("FRUSUARIOFRFUNCAO_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(p => p.FrUsuario);
            Map(b => b.FrFuncao);
            Map(b => b.FrEquipe);
            Map(b => b.Status);

            Table("FrUsuarioFrFuncao");
        }
    }
}
