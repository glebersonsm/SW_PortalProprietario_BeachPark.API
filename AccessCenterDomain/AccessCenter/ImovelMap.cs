using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class ImovelMap : ClassMap<Imovel>
    {
        public ImovelMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("IMOVEL_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(b => b.Numero);
            Map(b => b.TipoImovel);
            Map(b => b.ImovelAndar);
            Map(b => b.ImovelBloco);
            Map(b => b.ImovelVista);
            Map(b => b.ImovelLado);
            Map(b => b.GrupoCota);
            Map(b => b.CategoriaCota);
            Map(b => b.LiberadoVenda);
            Map(b => b.Empreendimento);
            Map(b => b.FracaoIdeal);
            Map(b => b.FracaoIdealM2);
            Map(b => b.AreaPrivativa);
            Map(b => b.AreaComum);
            Map(b => b.AreaTotal);
            Map(b => b.Capacidade);
            Map(b => b.QuantidadeQuartos);
            Map(b => b.FracaoIdealPool);
            Map(b => b.QuantidadeBanheiros);
            Map(b => b.QuantidadeCamas);
            Map(b => b.PossuiVaranda);
            Map(b => b.PossuiBanheira);
            Map(b => b.Pne);
            Map(b => b.FormatoDataEntrega);
            Map(b => b.QuantidadeMesesEntregaProduto);
            Map(b => b.DataEntregaProduto);
            Map(b => b.ApropriarReceitaMensal);
            Map(b => b.DataFimApropriacao);
            Map(b => b.FormatoDataApropriacao);
            Map(b => b.QuantidadeMesesFimApropriacao);

            Table("Imovel");
        }
    }
}
