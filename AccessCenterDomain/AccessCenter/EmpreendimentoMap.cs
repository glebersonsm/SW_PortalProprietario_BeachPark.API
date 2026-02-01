using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class EmpreendimentoMap : ClassMap<Empreendimento>
    {
        public EmpreendimentoMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("EMPREENDIMENTO_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(b => b.Filial);
            Map(b => b.GrupoEmpresa);
            Map(b => b.Empresa);
            Map(b => b.Codigo);
            Map(b => b.Nome);
            Map(b => b.NomePesquisa);
            Map(b => b.AlterarClienteFinanceiro);
            Map(b => b.TipoControlePeriodo);
            Map(b => b.AlterarCategoriaCotaFin);
            Map(b => b.Entregue);
            Map(b => b.MesPrazoEntrega);
            Map(b => b.AnoPrazoEntrega);
            Map(b => b.ValorEstimadoCondominio);
            Map(b => b.MesPrazoInicioCondominio);
            Map(b => b.TaxaPercentualUtilizacao);
            Map(b => b.Logradouro);
            Map(b => b.Numero);
            Map(b => b.Bairro);
            Map(b => b.Cep);
            Map(b => b.Complemento);
            Map(b => b.Cidade);
            Map(b => b.DataUltimoRefracionamento);
            Map(b => b.AlterarClienteFinanceiroCan);

            Table("Empreendimento");
        }
    }
}
