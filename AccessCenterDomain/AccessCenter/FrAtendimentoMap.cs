using AccessCenterDomain.AccessCenter.Fractional;
using FluentNHibernate.Mapping;

namespace AccessCenterDomain.AccessCenter
{
    public class FrAtendimentoMap : ClassMap<FrAtendimento>
    {
        public FrAtendimentoMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("FRATENDIMENTO_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(p => p.Filial);
            Map(b => b.LgCampanha);
            Map(b => b.LgPontoCaptacao);
            Map(b => b.LgQuestionario);
            Map(b => b.FrSala);
            Map(b => b.Status);
            Map(b => b.DataHoraFinalizacao);
            Map(b => b.DataHoraAtendimento);
            Map(b => b.UsuarioFinalizacao);
            Map(b => b.FrQualificacao);
            Map(b => b.TipoQualificacao);
            Map(b => b.GrupoEmpresa);
            Map(b => b.Empresa);
            Map(b => b.FrQualificacaoAutomatica);
            Map(b => b.FrPessoa1);
            Map(b => b.FrPessoa2);
            Map(b => b.Fase);
            Map(b => b.FaseStatus);
            Map(b => b.FTB);
            Map(b => b.IdPromotorTlmkt);
            Map(b => b.IdPromotor);
            Map(b => b.IdLiner);
            Map(b => b.IdCloser);
            Map(b => b.IdPep);
            Map(b => b.IdFtbSugerido);
            Map(b => b.IdLinerSugerido);
            Map(b => b.IdCloserSugerido);
            Map(b => b.IdPepSugerido);
            Map(b => b.FlgFtb);

            Table("FrAtendimento");
        }
    }
}
