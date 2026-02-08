using FluentNHibernate.Mapping;
using SW_PortalProprietario.Domain.Entities.Core.Sistema;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Infra.Data.Mappings.Core.Sistema
{
    public class ParametroSistemaMap : ClassMap<ParametroSistema>
    {
        public ParametroSistemaMap()
        {
            Id(x => x.Id).GeneratedBy.Native("ParametroSistema_");
            Map(x => x.ObjectGuid).Length(100);
            Map(p => p.UsuarioCriacao);

            Map(b => b.DataHoraCriacao);

            Map(b => b.DataHoraAlteracao)
                .Nullable();

            Map(p => p.UsuarioAlteracao)
                .Nullable();

            References(p => p.Empresa, "Empresa");
            Map(b => b.AgruparCertidaoPorCliente).CustomType<EnumSimNao>();
            Map(b => b.EmitirCertidaoPorUnidCliente).CustomType<EnumSimNao>();
            Map(b => b.HabilitarBaixarBoleto).CustomType<EnumSimNao>();
            Map(b => b.HabilitarPagamentosOnLine).CustomType<EnumSimNao>();
            Map(b => b.HabilitarPagamentoEmPix).CustomType<EnumSimNao>();
            Map(b => b.HabilitarPagamentoEmCartao).CustomType<EnumSimNao>();
            Map(b => b.ExibirContasVencidas).CustomType<EnumSimNao>();
            Map(b => b.QtdeMaximaDiasContasAVencer);
            Map(b => b.PermitirUsuarioAlterarSeuDoc).CustomType<EnumSimNao>();
            Map(b => b.PermitirUsuarioAlterarSeuEmail).CustomType<EnumSimNao>();
            Map(b => b.IntegradoComMultiPropriedade).CustomType<EnumSimNao>();
            Map(b => b.IntegradoComTimeSharing).CustomType<EnumSimNao>();
            Map(b => b.ImagemHomeUrl1).Length(500);
            Map(b => b.ImagemHomeUrl2).Length(500);
            Map(b => b.ImagemHomeUrl3).Length(500);
            Map(b => b.ImagemHomeUrl4).Length(500);
            Map(b => b.ImagemHomeUrl5).Length(500);
            Map(b => b.ImagemHomeUrl6).Length(500);
            Map(b => b.ImagemHomeUrl7).Length(500);
            Map(b => b.ImagemHomeUrl8).Length(500);
            Map(b => b.ImagemHomeUrl9).Length(500);
            Map(b => b.ImagemHomeUrl10).Length(500);
            Map(b => b.ImagemHomeUrl11).Length(500);
            Map(b => b.ImagemHomeUrl12).Length(500);
            Map(b => b.ImagemHomeUrl13).Length(500);
            Map(b => b.ImagemHomeUrl14).Length(500);
            Map(b => b.ImagemHomeUrl15).Length(500);
            Map(b => b.ImagemHomeUrl16).Length(500);
            Map(b => b.ImagemHomeUrl17).Length(500);
            Map(b => b.ImagemHomeUrl18).Length(500);
            Map(b => b.ImagemHomeUrl19).Length(500);
            Map(b => b.ImagemHomeUrl20).Length(500);
            Map(b => b.SiteParaReserva).Length(500);
            Map(p => p.NomeCondominio);
            Map(p => p.CnpjCondominio);
            Map(p => p.EnderecoCondominio);
            Map(p => p.ExibirFinanceirosDasEmpresaIds);
            Map(p => p.ExigeDocumentoHospedeConvidado).CustomType<EnumSimNao>();
            Map(p => p.ExigeEnderecoHospedeConvidado).CustomType<EnumSimNao>();
            Map(p => p.ExigeTelefoneHospedeConvidado).CustomType<EnumSimNao>();

            Map(p => p.NomeAdministradoraCondominio);
            Map(p => p.CnpjAdministradoraCondominio);
            Map(p => p.EnderecoAdministradoraCondominio);
            Map(b => b.PontosRci);
            Map(b => b.PermiteReservaRciApenasClientesComContratoRci).CustomType<EnumSimNao>();

            Map(b => b.Habilitar2FAPorEmail).CustomType<EnumSimNao>();
            Map(b => b.Habilitar2FAPorSms).CustomType<EnumSimNao>();
            Map(b => b.Habilitar2FAParaCliente).CustomType<EnumSimNao>();
            Map(b => b.Habilitar2FAParaAdministrador).CustomType<EnumSimNao>();
            Map(b => b.EndpointEnvioSms2FA).Length(500);

            Map(b => b.SmtpHost).Length(500);
            Map(b => b.SmtpPort);
            Map(b => b.SmtpUseSsl).CustomType<EnumSimNao>();
            Map(b => b.SmtpUser).Length(500);
            Map(b => b.SmtpPass).Length(500);
            Map(b => b.SmtpFromName).Length(500);

            Table("ParametroSistema");
        }
    }
}
