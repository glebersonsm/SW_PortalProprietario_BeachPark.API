using AccessCenterDomain.AccessCenter.Fractional;
using FluentNHibernate.Mapping;

namespace AccessCenterDomain.AccessCenter
{
    public class FrAtendimentoBrindeMap : ClassMap<FrAtendimentoBrinde>
    {
        public FrAtendimentoBrindeMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("FRATENDIMENTOBRINDE_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(b => b.FrAtendimento);
            Map(b => b.Fase);
            Map(b => b.FrBrinde);
            Map(b => b.FrComboBrinde);
            Map(b => b.Quantidade);
            Map(b => b.Status);
            Map(b => b.Pessoa1);
            Map(b => b.Pessoa2);
            Map(b => b.FrSala);
            Map(b => b.FrSolicitacaoNegociacao);
            Map(b => b.DataHoraCancelamento);
            Map(b => b.DataHoraConcessao);
            Map(b => b.UsuarioConcessao);
            Map(b => b.VoucherIntegracao);
            Map(b => b.Utilizado);
            Map(b => b.DataHoraUtilizacao);
            Map(b => b.DataUtilizacao);
            Map(b => b.UsuarioUtilizacao);
            Map(b => b.FrBrindePagamento);
            Map(b => b.ValorPagamento);

            Table("FrAtendimentoBrinde");
        }
    }
}
