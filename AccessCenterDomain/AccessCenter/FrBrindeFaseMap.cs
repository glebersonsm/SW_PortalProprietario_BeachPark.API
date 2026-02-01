using FluentNHibernate.Mapping;

namespace AccessCenterDomain.AccessCenter
{
    public class FrBrindeFaseMap : ClassMap<FrBrindeFase>
    {
        public FrBrindeFaseMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("FRBRINDEFASE_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(b => b.Filial);
            Map(b => b.Nome);
            Map(b => b.NomePesquisa);
            Map(b => b.PermitirUtilizarAbordagem);
            Map(b => b.PermitirUtilizarSala);
            Map(b => b.PermitirUtilizarRecepcao);
            Map(b => b.PermitirUtilizarNegociacao);
            Map(b => b.PermitirUtilizarAvulso);
            Map(b => b.Status);

            Table("FrBrindeFase");
        }
    }
}
