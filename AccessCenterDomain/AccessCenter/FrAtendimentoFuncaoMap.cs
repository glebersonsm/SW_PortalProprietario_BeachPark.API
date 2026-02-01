using AccessCenterDomain.AccessCenter.Fractional;
using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class FrAtendimentoFuncaoMap : ClassMap<FrAtendimentoFuncao>
    {
        public FrAtendimentoFuncaoMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("FRATENDIMENTOFUNCAO_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(p => p.FrAtendimento);
            Map(b => b.FrFuncao);
            Map(b => b.InformarUsuario);
            Map(b => b.UtilizouPerfilAnalise);
            Map(b => b.FrUsuario);

            Table("FrAtendimentoFuncao");
        }
    }
}
