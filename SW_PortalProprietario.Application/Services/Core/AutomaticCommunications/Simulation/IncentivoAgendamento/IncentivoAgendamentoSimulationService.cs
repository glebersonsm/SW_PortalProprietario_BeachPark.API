using Microsoft.Extensions.Logging;
using SW_PortalProprietario.Application.Interfaces;
using SW_PortalProprietario.Application.Models;
using SW_PortalProprietario.Application.Models.Empreendimento;
using SW_PortalProprietario.Application.Models.GeralModels;
using SW_PortalProprietario.Application.Services.Core.AutomaticCommunications.GenerationServices;
using SW_PortalProprietario.Application.Services.Core.Interfaces;
using SW_PortalProprietario.Application.Services.Providers.Interfaces;
using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Services.Core.AutomaticCommunications.Simulation.IncentivoAgendamento;

/// <summary>
/// Serviço auxiliar para simulação de emails de incentivo para agendamento
/// ? USA O MESMO CÓDIGO DO PROCESSAMENTO AUTOMÁTICO via IncentivoAgendamentoGenerationService
/// </summary>
public class IncentivoAgendamentoSimulationService
{
    private readonly ILogger<IncentivoAgendamentoSimulationService> _logger;
    private readonly IServiceBase _serviceBase;
    private readonly IEmpreendimentoHybridProviderService _empreendimentoProviderService;
    private readonly IncentivoAgendamentoGenerationService _generationService;

    public IncentivoAgendamentoSimulationService(
        ILogger<IncentivoAgendamentoSimulationService> logger,
        IServiceBase serviceBase,
        IEmpreendimentoHybridProviderService empreendimentoProviderService,
        IncentivoAgendamentoGenerationService generationService)
    {
        _logger = logger;
        _serviceBase = serviceBase;
        _empreendimentoProviderService = empreendimentoProviderService;
        _generationService = generationService;
    }

    public async Task<List<EmailInputInternalModel>> GenerateSimulationEmailAsync(
        AutomaticCommunicationConfigModel config,
        string userEmail,
        int userId)
    {
        var listResult = new List<EmailInputInternalModel>();

        _logger.LogInformation("=== INÍCIO SIMULAÇÃO INCENTIVO PARA AGENDAMENTO ===");

        if (!config.DaysBeforeCheckIn.Any())
            throw new ArgumentException("Configuração inválida: Dias disparo envio está vazio.");

        var contratos = await _serviceBase.GetContratos(new List<int>());
        var inadimplentes = await _empreendimentoProviderService.Inadimplentes();

        _logger.LogInformation("Buscando contratos elegíveis para incentivo de agendamento");

        var ano = DateTime.Now.Month >= 6 ? DateTime.Now.Year + 1 : DateTime.Now.Year;
        var contratosElegiveis = await _generationService.GetContratosElegiveisAsync(
            (EnumProjetoType)config.ProjetoType,
            config,
            contratos ?? new List<DadosContratoModel>(),
            ano,
            simulacao: true);

        if (contratosElegiveis == null || !contratosElegiveis.Any())
            throw new ArgumentException("Nenhum contrato elegível encontrado para simulação de incentivo para agendamento");

        _logger.LogInformation("Encontrados {Count} contratos elegíveis para análise", contratosElegiveis.Count);

        foreach (var item in contratosElegiveis)
        {
            var contratoCompativel = await FindCompatibleContratoAsync(
                new List<(DadosContratoModel contrato, PosicaoAgendamentoViewModel statusAgendamento, int intervalo)>() { item },
                config,
                inadimplentes,
                simulacao: true);

            if (contratoCompativel == null || !contratoCompativel.HasValue ||
                contratoCompativel.Value.contrato == null || contratoCompativel.Value.statusAgendamento == null)
                continue;

            _logger.LogInformation("Contrato selecionado: NumeroContrato={NumeroContrato}, Titular={Titular}",
                contratoCompativel.Value.contrato.NumeroContrato, contratoCompativel.Value.contrato.PessoaTitular1Nome);

            // ? GERAR CONTEÚDO DO EMAIL - MESMA LÓGICA DO PROCESSAMENTO AUTOMÁTICO
            var emailData = await _generationService.GerarAvisoCompletoAsync(
                config,
                contratoCompativel.Value.contrato,
                contratoCompativel.Value.statusAgendamento,
                item.intervalo);

            if (emailData == null)
                throw new ArgumentException("Erro ao gerar conteúdo do email para simulação de incentivo para agendamento");

            _logger.LogInformation("Conteúdo do email gerado com sucesso");

            var result = new EmailInputInternalModel
            {
                Assunto = $"[SIMULAÇÃO] {emailData.Subject ?? "Incentivo ao Agendamento"}",
                Destinatario = userEmail,
                ConteudoEmail = emailData.HtmlContent ?? "Incentivo ao Agendamento",
                EmpresaId = 1,
                UsuarioCriacao = userId,
                Anexos = null
            };

            // Adicionar anexo se modo configurado incluir anexo E tiver PDF
            if ((config.TemplateSendMode == EnumTemplateSendMode.AttachmentOnly ||
                config.TemplateSendMode == EnumTemplateSendMode.BodyHtmlAndAttachment)
                && emailData.PdfBytes != null && emailData.PdfBytes.Length > 0)
            {
                result.Anexos = new List<EmailAnexoInputModel>
                {
                    new EmailAnexoInputModel
                    {
                        NomeArquivo = emailData.PdfFileName!,
                        TipoMime = "application/pdf",
                        Arquivo = emailData.PdfBytes
                    }
                };

                _logger.LogInformation("Anexo PDF adicionado ao email: {FileName} ({Size} bytes)",
                    emailData.PdfFileName, emailData.PdfBytes.Length);
            }

            listResult.Add(result);
        }

        _logger.LogInformation("=== FIM SIMULAÇÃO INCENTIVO PARA AGENDAMENTO ===");

        return listResult;
    }

    #region Métodos Auxiliares

    private async Task<(DadosContratoModel contrato, PosicaoAgendamentoViewModel statusAgendamento, int intervalo)?> FindCompatibleContratoAsync(
        List<(DadosContratoModel contrato, PosicaoAgendamentoViewModel statusAgendamento, int intervalo)> contratos,
        AutomaticCommunicationConfigModel config,
        List<ClientesInadimplentes> inadimplentes,
        bool simulacao = false)
    {
        foreach (var contratoItem in contratos)
        {
            var email = contratoItem.contrato.PessoaTitular1Email ?? contratoItem.contrato.PessoaTitular2Email;
            if (!IsValidEmail(email))
                continue;

            if (!simulacao)
            {
                if (!await _generationService.ShouldSendEmailForContrato(
                    contratoItem.contrato, config, inadimplentes))
                    continue;
            }

            return contratoItem;
        }

        return null;
    }

    private bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    #endregion
}
