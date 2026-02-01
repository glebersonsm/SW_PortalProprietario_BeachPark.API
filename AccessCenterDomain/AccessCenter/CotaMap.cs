using FluentNHibernate.Mapping;


namespace AccessCenterDomain.AccessCenter
{
    public class CotaMap : ClassMap<Cota>
    {
        public CotaMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("COTA_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(b => b.GrupoCotaTipoCota);
            Map(b => b.Imovel);
            Map(b => b.Proprietario);
            Map(b => b.Procurador);
            Map(b => b.Status);
            Map(b => b.FrAtendimentoVenda);
            Map(b => b.CategoriaCota);
            Map(b => b.DataAquisicao);
            Map(b => b.Bloqueado);
            Map(b => b.ObservacaoBloqueio);
            Map(b => b.Observacao);
            Map(b => b.Percentual);
            Map(b => b.PagaCondominio);
            Map(b => b.SequenciaVenda);

            Table("Cota");
        }
    }
}
