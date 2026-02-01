using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class TipoContaPagarMap : ClassMap<TipoContaPagar>
    {
        public TipoContaPagarMap()
        {
            Id(x => x.Id).GeneratedBy.Sequence("TIPOCONTAPAGAR_SEQUENCE");
            Map(b => b.Tag);
            Map(b => b.UsuarioCriacao);
            Map(b => b.DataHoraCriacao);
            Map(b => b.UsuarioAlteracao);
            Map(b => b.DataHoraAlteracao);

            Map(b => b.Empresa);
            Map(b => b.Codigo);
            Map(b => b.Nome);
            Map(b => b.NomePesquisa);
            Map(b => b.Status);
            Map(b => b.Adiantamento);
            Map(b => b.lancaMovFinanceiraPagto);
            Map(b => b.AFaturar);
            Map(b => b.PermitirLactoDebito);
            Map(b => b.TipoAgrupamento);
            Map(b => b.ApareceExtratoContaPagar);
            Map(b => b.RestringeTipoCliente);
            Map(b => b.LancaBloqueado);
            Map(b => b.RetemImpostoRenda);
            Map(b => b.PermitirLancamentoManual);
            Map(b => b.EncontroContasAutomatico);
            Map(b => b.RestringeLiberacaoPorUsuario);

            Table("TipoContaPagar");
        }
    }
}
