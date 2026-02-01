using FluentNHibernate.Mapping;

namespace AccessCenterDomain.AccessCenter
{
    public class FrBrindeMap : ClassMap<FrBrinde>
    {
        public FrBrindeMap()
        {
            Id(x => x.Id)
            .GeneratedBy.Sequence("FRBRINDE_");

            Map(b => b.Tag);
            Map(p => p.DataHoraCriacao);
            Map(p => p.UsuarioCriacao);
            Map(p => p.DataHoraAlteracao);
            Map(p => p.UsuarioAlteracao);

            Map(p => p.Filial);

            Map(b => b.Codigo);
            Map(b => b.Nome);
            Map(b => b.NomePesquisa);
            Map(b => b.Status);
            Map(b => b.ControlaEstoque);
            Map(b => b.ProdutoItem);
            Map(b => b.Terceiro);
            Map(b => b.ClienteTerceiro);
            Map(b => b.Valor);
            Map(b => b.ValorTerceiro);
            Map(b => b.Descricao);
            Map(b => b.EspecificacaoUso);
            Map(b => b.FrTipoDocumentoImpressao);
            Map(b => b.QuantidadeMaximaAbordagem);
            Map(b => b.QuantidadeMaximaSala);
            Map(b => b.ParticipaIntegracao);
            Map(b => b.EnviarSmsConfirmacao);
            Map(b => b.EnviarSmsLinkVoucher);
            Map(b => b.VoucherUnico);
            Map(b => b.ProvedorSms);
            Map(b => b.SmsModelo);
            Map(b => b.EnviarEmailLinkVoucher);
            Map(b => b.EmailModelo);
            Map(b => b.EmailRemetente);
            Map(b => b.TipoDocumento);
            Map(b => b.Vencimento);
            Map(b => b.UtilizaMensagemWhatsApp);
            Map(b => b.MensagemWhatsapp);

            Table("FrBrinde");
        }
    }
}
