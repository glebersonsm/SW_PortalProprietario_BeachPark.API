using System.Collections.ObjectModel;

namespace SW_PortalProprietario.Application.Models.DocumentTemplates;

/// <summary>
/// Lista de chaves disponÃ­veis para montagem do template de incentivo para agendamento.
/// As chaves devem ser utilizadas no arquivo DOCX no formato {{NomeDaChave}}.
/// </summary>
public static class IncentivoParaAgendamentoPlaceholder
{
    // Dados do Cliente/Contrato
    public const string NomeCliente = "{{NomeCliente}}";
    public const string DocumentoCliente = "{{DocumentoCliente}}";
    public const string ContratoNumero = "{{ContratoNumero}}";
    
    // Dados do PerÃ­odo de Agendamento
    public const string DataInicioAgendamento = "{{DataInicioAgendamento}}";
    public const string DataFinalAgendamento = "{{DataFinalAgendamento}}";
    public const string PeriodoAgendamentoFormatado = "{{PeriodoAgendamentoFormatado}}";
    public const string AnoReferencia = "{{AnoReferencia}}";
    
    // Dados das Semanas
    public const string QuantidadeSemanasDireito = "{{QuantidadeSemanasDireito}}";
    public const string QuantidadeSemanasAgendadas = "{{QuantidadeSemanasAgendadas}}";
    public const string QuantidadeSemanasDisponiveis = "{{QuantidadeSemanasDisponiveis}}";
    
    // Dados do Sistema
    public const string DataGeracao = "{{DataGeracao}}";
    public const string LinkPortal = "{{LinkPortal}}";
    public const string ContatoCentralAtendimento = "{{ContatoCentralAtendimento}}";

    private static readonly IReadOnlyCollection<PlaceholderDescriptionIncentivo> _all = new ReadOnlyCollection<PlaceholderDescriptionIncentivo>(
        new[]
        {
            new PlaceholderDescriptionIncentivo(NomeCliente, "Nome do cliente proprietÃ¡rio."),
            new PlaceholderDescriptionIncentivo(DocumentoCliente, "CPF/CNPJ do cliente."),
            new PlaceholderDescriptionIncentivo(ContratoNumero, "NÃºmero do contrato."),
            
            new PlaceholderDescriptionIncentivo(DataInicioAgendamento, "Data de inÃ­cio do perÃ­odo de agendamento."),
            new PlaceholderDescriptionIncentivo(DataFinalAgendamento, "Data final do perÃ­odo de agendamento."),
            new PlaceholderDescriptionIncentivo(PeriodoAgendamentoFormatado, "PerÃ­odo de agendamento formatado (ex: 01/01/2024 a 31/12/2024)."),
            new PlaceholderDescriptionIncentivo(AnoReferencia, "Ano de referÃªncia do agendamento."),
            
            new PlaceholderDescriptionIncentivo(QuantidadeSemanasDireito, "Total de semanas que o cliente tem direito no ano."),
            new PlaceholderDescriptionIncentivo(QuantidadeSemanasAgendadas, "Quantidade de semanas jÃ¡ agendadas pelo cliente."),
            new PlaceholderDescriptionIncentivo(QuantidadeSemanasDisponiveis, "Quantidade de semanas ainda disponÃ­veis para agendamento."),
            
            new PlaceholderDescriptionIncentivo(DataGeracao, "Data de geraÃ§Ã£o do documento."),
            new PlaceholderDescriptionIncentivo(LinkPortal, "Link para acessar o portal do proprietÃ¡rio."),
            new PlaceholderDescriptionIncentivo(ContatoCentralAtendimento, "InformaÃ§Ãµes de contato da Central de Atendimento.")
        });

    public static IReadOnlyCollection<PlaceholderDescriptionIncentivo> All => _all;
}

public record PlaceholderDescriptionIncentivo(string Key, string Description);