using FluentNHibernate.Mapping;

namespace AccessCenterDomain.AccessCenter
{
    public class ClienteMap : ClassMap<Cliente>
    {
        public ClienteMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("CLIENTE_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);


            Map(b => b.Empresa);
            Map(b => b.GrupoEmpresa);
            Map(b => b.Pessoa);
            Map(b => b.Filial);
            Map(b => b.FilialVinculada);
            Map(p => p.Codigo);
            Map(p => p.Status);
            Map(p => p.ExigeOrdemCompra);
            Map(p => p.RestringeTipoRecebimento);
            Map(p => p.RestringeCondicaoVenda);
            Map(p => p.PermiteVendaSemCartao);
            Map(p => p.PossuiFazenda);
            Map(p => p.EmailNFe);
            Map(p => p.TipoClienteClassificacao);
            Map(p => p.TipoClientePrioritario);
            Map(p => p.RestringeNaturezaOperacao);
            Map(p => p.ObrigaXmlNfe);
            Map(p => p.PermiteLibSemLimPeloGer);
            Map(p => p.ContribuinteIcms);
            Map(p => p.CondominioUsuario);
            Map(p => p.CondominioSenha);

            Table("Cliente");
        }
    }
}
