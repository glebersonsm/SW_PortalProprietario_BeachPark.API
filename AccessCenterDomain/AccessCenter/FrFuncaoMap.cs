using AccessCenterDomain.AccessCenter.Fractional;
using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class FrFuncaoMap : ClassMap<FrFuncao>
    {
        public FrFuncaoMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("FRFUNCAO_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(b => b.GrupoEmpresa);
            Map(b => b.CargoPedido);
            Map(b => b.GeraContaPagar);
            Map(b => b.FTB);
            Map(b => b.LancamentoAutomatico);
            Map(b => b.Fase);
            Map(b => b.FaseStatus);
            Map(b => b.ExigeEquipe);
            Map(b => b.PermitirSomenteUmNaEquipe);
            Map(b => b.PermitirVinculoMaisDeUmaEquipe);
            Map(b => b.ExibeContrato);
            Map(b => b.ExibeCentralAtendimento);
            Map(b => b.FaseRelatorioBrindeConcedido);
            Map(b => b.Codigo);
            Map(b => b.Nome);
            Map(b => b.NomePesquisa);
            Map(b => b.SwVinculos);

            Table("FrFuncao");
        }
    }
}
