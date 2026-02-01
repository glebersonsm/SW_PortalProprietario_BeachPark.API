using AccessCenterDomain.AccessCenter.Fractional;
using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class FrCrcAtendimentoMap : ClassMap<FrCrcAtendimento>
    {
        public FrCrcAtendimentoMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("FRCRCATENDIMENTO_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(p => p.Filial);
            Map(b => b.Codigo);
            Map(b => b.Cliente);
            Map(b => b.FrAtendimentoVenda);
            Map(b => b.DataHoraInicio);
            Map(b => b.DataHoraFim);
            Map(b => b.FrCrcTipoAtendimento);
            Map(b => b.FrSala);
            Map(b => b.Status);
            Map(b => b.Descricao);
            Map(b => b.Observacao);
            Map(b => b.ObservacaoCompleta);
            Map(b => b.GrupoEmpresa);
            Map(b => b.Empresa);
            Map(b => b.IntegracaoId);
            Map(b => b.FrCrcMeioComunicacao);
            Map(b => b.FrCrcResultadoAtendimento);

            Table("FrCrcAtendimento");
        }
    }
}
