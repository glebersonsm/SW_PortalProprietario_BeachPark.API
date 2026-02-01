using AccessCenterDomain.AccessCenter.Fractional;
using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class FrCrcMeioComunicacaoMap : ClassMap<FrCrcMeioComunicacao>
    {
        public FrCrcMeioComunicacaoMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("FRCRCATENDIMENTO_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(p => p.GrupoEmpresa);
            Map(b => b.Codigo);
            Map(b => b.Nome);
            Map(b => b.NomePesquisa);

            Table("FrCrcMeioComunicacao");
        }
    }
}
