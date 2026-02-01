namespace SW_PortalProprietario.Application.Models.DocumentTemplates;

public class DadosIncentivoAgendamentoModel
{
    // Dados do Cliente/Contrato
    public string NomeCliente { get; set; } = string.Empty;
    public string DocumentoCliente { get; set; } = string.Empty;
    public string ContratoNumero { get; set; } = string.Empty;
    public string EmailCliente { get; set; } = string.Empty;
    public string TelefoneCliente { get; set; } = string.Empty;
    
    // Dados do Período de Agendamento
    public DateTime DataInicioAgendamento { get; set; }
    public DateTime DataFinalAgendamento { get; set; }
    public string PeriodoAgendamentoFormatado { get; set; } = string.Empty;
    public int AnoReferencia { get; set; }
    
    // Dados das Semanas
    public int QuantidadeSemanasDireito { get; set; }
    public int QuantidadeSemanasAgendadas { get; set; }
    public int QuantidadeSemanasDisponiveis { get; set; }
    public decimal PercentualUtilizacao { get; set; }
    
    // Dados das Semanas por Ano
    public List<SemanaPorAnoModel> ListaSemanasDisponiveis { get; set; } = new();
    public string ResumoSemanasAnual { get; set; } = string.Empty;
    
    // Dados de Incentivo
    public string MensagemIncentivo { get; set; } = string.Empty;
    public DateTime? DataLimiteAgendamento { get; set; }
    public List<string> BeneficiosAgendamento { get; set; } = new();
    
    // Dados do Sistema
    public DateTime DataGeracao { get; set; }
    public string LinkPortal { get; set; } = string.Empty;
    public string ContatoSuporte { get; set; } = string.Empty;
}

public class SemanaPorAnoModel
{
    public int Ano { get; set; }
    public int SemanasDireito { get; set; }
    public int SemanasAgendadas { get; set; }
    public int SemanasDisponiveis => SemanasDireito - SemanasAgendadas;
    public decimal PercentualUtilizacao => SemanasDireito > 0 ? (decimal)SemanasAgendadas / SemanasDireito * 100 : 0;
}