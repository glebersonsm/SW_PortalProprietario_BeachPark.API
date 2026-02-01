using System.Collections.ObjectModel;

namespace SW_PortalProprietario.Application.Models.DocumentTemplates;

/// <summary>
/// Lista de chaves disponíveis para montagem do template de confirmação de cancelamento de Reserva RCI
/// As chaves devem ser utilizadas no arquivo DOCX no formato {{NomeDaChave}}.
/// </summary>
public static class ComunicacaoRelacionadaAoContrato
{
    public const string NomeCliente = "{{NomeCliente}}";
    public const string ContratoNumero = "{{ContratoNumero}}";
    public const string DataSolicitacaoAgendamento = "{{DataSolicitacaoAgendamento}}";
    public const string TotalPago = "{{TotalPago}}";
    public const string TotalAVencer = "{{TotalAVencer}}";
    public const string TotalVencido = "{{TotalVencido}}";
    public const string ListaParcelasVencidas = "{{ListaParcelasVencidas}}";
    public const string DataAberturaCalendarioAgendamento = "{{DataAberturaCalendarioPrioridade}}";
    public const string DataFechamentoCalendarioPrioridade = "{{DataFechamentoCalendarioPrioridade}}";
    public const string IdRci = "{{IdRci}}";
    

    private static readonly IReadOnlyCollection<PlaceholderDescriptionContract> _all = new ReadOnlyCollection<PlaceholderDescriptionContract>(
        new[]
        {
            new PlaceholderDescriptionContract(NomeCliente, "Nome do cliente (proprietário principal)."),
            new PlaceholderDescriptionContract(ContratoNumero, "Número do contrato."),
            new PlaceholderDescriptionContract(DataSolicitacaoAgendamento, "Data de solicitação do agendamento."),
            new PlaceholderDescriptionContract(IdRci, "Documento do co-cessionário."),
            new PlaceholderDescriptionContract(TotalPago, "Valor total pago do contrato."),
            new PlaceholderDescriptionContract(TotalAVencer, "Valor total a vencer do contrato."),
            new PlaceholderDescriptionContract(TotalVencido, "Valor total vencido do contrato."),
            new PlaceholderDescriptionContract(ListaParcelasVencidas, "Lista das parcelas vencidas do contrato."),
            new PlaceholderDescriptionContract(DataAberturaCalendarioAgendamento, "Data de abertura do calendário de prioridade."),
            new PlaceholderDescriptionContract(DataFechamentoCalendarioPrioridade, "Data de fechamento do calendário de prioridade.")
        });

    public static IReadOnlyCollection<PlaceholderDescriptionContract> All => _all;
}

public record PlaceholderDescriptionContract(string Key, string Description);

