using SW_PortalProprietario.Domain.Entities.Core.Framework;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Domain.Entities.Core.Sistema
{
    public class ParametroSistema : EntityBaseCore, IEntityValidateCore
    {
        public virtual Empresa? Empresa { get; set; }
        public virtual string? SiteParaReserva { get; set; }
        public virtual EnumSimNao? AgruparCertidaoPorCliente { get; set; } = EnumSimNao.Não;
        public virtual EnumSimNao? EmitirCertidaoPorUnidCliente { get; set; } = EnumSimNao.Não;
        public virtual EnumSimNao? HabilitarBaixarBoleto { get; set; } = EnumSimNao.Não;
        public virtual EnumSimNao? HabilitarPagamentosOnLine { get; set; } = EnumSimNao.Não;
        public virtual EnumSimNao? HabilitarPagamentoEmPix { get; set; } = EnumSimNao.Não;
        public virtual EnumSimNao? HabilitarPagamentoEmCartao { get; set; } = EnumSimNao.Não;
        public virtual EnumSimNao? ExibirContasVencidas { get; set; } = EnumSimNao.Não;
        public virtual int? QtdeMaximaDiasContasAVencer { get; set; }
        public virtual EnumSimNao? PermitirUsuarioAlterarSeuEmail { get; set; } = EnumSimNao.Não;
        public virtual EnumSimNao? PermitirUsuarioAlterarSeuDoc { get; set; } = EnumSimNao.Não;
        public virtual EnumSimNao? IntegradoComMultiPropriedade { get; set; } = EnumSimNao.Não;
        public virtual EnumSimNao? IntegradoComTimeSharing { get; set; } = EnumSimNao.Não;
        #region Imagens empreendimetno I
        public virtual string? ImagemHomeUrl1 { get; set; }
        public virtual string? ImagemHomeUrl2 { get; set; }
        public virtual string? ImagemHomeUrl3 { get; set; }
        public virtual string? ImagemHomeUrl4 { get; set; }
        public virtual string? ImagemHomeUrl5 { get; set; }
        public virtual string? ImagemHomeUrl6 { get; set; }
        public virtual string? ImagemHomeUrl7 { get; set; }
        public virtual string? ImagemHomeUrl8 { get; set; }
        public virtual string? ImagemHomeUrl9 { get; set; }
        public virtual string? ImagemHomeUrl10 { get; set; } 
        #endregion

        #region Imagens empreendimento II
        public virtual string? ImagemHomeUrl11 { get; set; }
        public virtual string? ImagemHomeUrl12 { get; set; }
        public virtual string? ImagemHomeUrl13 { get; set; }
        public virtual string? ImagemHomeUrl14 { get; set; }
        public virtual string? ImagemHomeUrl15 { get; set; }
        public virtual string? ImagemHomeUrl16 { get; set; }
        public virtual string? ImagemHomeUrl17 { get; set; }
        public virtual string? ImagemHomeUrl18 { get; set; }
        public virtual string? ImagemHomeUrl19 { get; set; }
        public virtual string? ImagemHomeUrl20 { get; set; }
        public virtual string? NomeCondominio { get; set; } 
        #endregion
        public virtual string? CnpjCondominio { get; set; }
        public virtual string? EnderecoCondominio { get; set; }

        public virtual string? NomeAdministradoraCondominio { get; set; }
        public virtual string? CnpjAdministradoraCondominio { get; set; }
        public virtual string? EnderecoAdministradoraCondominio { get; set; }
        public virtual string? ExibirFinanceirosDasEmpresaIds { get; set; }
        public virtual int? PontosRci { get; set; } = 5629;
        
        #region Configurações de Reserva - Campos Obrigatórios para Hóspedes Convidados
        public virtual EnumSimNao? ExigeEnderecoHospedeConvidado { get; set; } = EnumSimNao.Não;
        public virtual EnumSimNao? ExigeTelefoneHospedeConvidado { get; set; } = EnumSimNao.Não;
        public virtual EnumSimNao? ExigeDocumentoHospedeConvidado { get; set; } = EnumSimNao.Não;
        #endregion

        #region Configurações de Reserva RCI
        public virtual EnumSimNao? PermiteReservaRciApenasClientesComContratoRci { get; set; } = EnumSimNao.Não;
        #endregion

        #region Autenticação em duas etapas (2FA)
        public virtual EnumSimNao? Habilitar2FAPorEmail { get; set; } = EnumSimNao.Não;
        public virtual EnumSimNao? Habilitar2FAPorSms { get; set; } = EnumSimNao.Não;
        public virtual EnumSimNao? Habilitar2FAParaCliente { get; set; } = EnumSimNao.Não;
        public virtual EnumSimNao? Habilitar2FAParaAdministrador { get; set; } = EnumSimNao.Não;
        /// <summary> URL do endpoint de envio de SMS para token 2FA (ex.: http://host:porta/cxf/sms/rest/enviar). Configurável no cadastro; nada interno no código. </summary>
        public virtual string? EndpointEnvioSms2FA { get; set; }
        #endregion

        public virtual async Task SaveValidate()
        {
            List<string> mensagens = new();

            if (Empresa == null)
                mensagens.Add("A empresa deve ser informada.");

            if (mensagens.Any())
                await Task.FromException(new ArgumentException(string.Join($"{Environment.NewLine}", mensagens.Select(a => a).ToList())));
            else await Task.CompletedTask;
        }

        public virtual object Clone()
        {
            return MemberwiseClone();
        }

    }
}
