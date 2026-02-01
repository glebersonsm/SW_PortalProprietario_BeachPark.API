using System.Collections.ObjectModel;

namespace SW_PortalProprietario.Application.Models.DocumentTemplates;

/// <summary>
/// Lista de chaves disponíveis para montagem do template de voucher de reserva.
/// As chaves devem ser utilizadas no arquivo DOCX no formato {{NomeDaChave}}.
/// </summary>
public static class VoucherReservaPlaceholder
{
    public const string NomeCliente = "{{NomeCliente}}";
    public const string DocumentoCliente = "{{DocumentoCliente}}";
    public const string NomeCoCessionario = "{{NomeCoCessionario}}";
    public const string DocumentoCoCessionario = "{{DocumentoCoCessionario}}";
    public const string ContratoNumero = "{{ContratoNumero}}";
    public const string ReservaNumero = "{{ReservaNumero}}";
    public const string HotelNome = "{{HotelNome}}";
    public const string CheckInData = "{{CheckInData}}";
    public const string CheckInHora = "{{CheckInHora}}";
    public const string CheckOutData = "{{CheckOutData}}";
    public const string CheckOutHora = "{{CheckOutHora}}";
    public const string HospedePrincipalNome = "{{HospedePrincipalNome}}";
    public const string HospedePrincipalDocumento = "{{HospedePrincipalDocumento}}";
    public const string TipoUtilizacao = "{{TipoUtilizacao}}";
    public const string TipoDisponibilizacao = "{{TipoDisponibilizacao}}";
    public const string TipoApartamento = "{{TipoApartamento}}";
    public const string OcupacaoMaxima = "{{OcupacaoMaxima}}";
    public const string VagaEstacionamento = "{{VagaEstacionamento}}";
    public const string HospedesLista = "{{HospedesLista}}";
    public const string TermoCessao = "{{TermoCessao}}";
    public const string Observacoes = "{{Observacoes}}";
    public const string QuantidadePaxPorFaixaEtaria = "{{QuantidadePaxPorFaixaEtaria}}";
    public const string LocalAtendimento = "{{LocalAtendimento}}";

    private static readonly IReadOnlyCollection<PlaceholderDescriptionReservas> _all = new ReadOnlyCollection<PlaceholderDescriptionReservas>(
        new[]
        {
            new PlaceholderDescriptionReservas(NomeCliente, "Nome do cliente (proprietário principal)."),
            new PlaceholderDescriptionReservas(DocumentoCliente, "Documento (CPF/CNPJ) do cliente."),
            new PlaceholderDescriptionReservas(NomeCoCessionario, "Nome do co-cessionário."),
            new PlaceholderDescriptionReservas(DocumentoCoCessionario, "Documento do co-cessionário."),
            new PlaceholderDescriptionReservas(ContratoNumero, "Código do contrato vinculado à reserva."),
            new PlaceholderDescriptionReservas(ReservaNumero, "Número da reserva gerado pela TSE."),
            new PlaceholderDescriptionReservas(HotelNome, "Nome do hotel ou empreendimento."),
            new PlaceholderDescriptionReservas(CheckInData, "Data do check-in formatada (dd/MM/yyyy)."),
            new PlaceholderDescriptionReservas(CheckInHora, "Horário de início permitido para check-in."),
            new PlaceholderDescriptionReservas(CheckOutData, "Data do check-out formatada (dd/MM/yyyy)."),
            new PlaceholderDescriptionReservas(CheckOutHora, "Horário limite para check-out."),
            new PlaceholderDescriptionReservas(HospedePrincipalNome, "Nome do hóspede principal informado na reserva."),
            new PlaceholderDescriptionReservas(HospedePrincipalDocumento, "Documento do hóspede principal."),
            new PlaceholderDescriptionReservas(TipoUtilizacao, "Tipo de utilização (Uso Próprio, Uso Convidado, Intercambiadora)."),
            new PlaceholderDescriptionReservas(TipoDisponibilizacao, "Descrição complementar do tipo de utilização."),
            new PlaceholderDescriptionReservas(TipoApartamento, "Descrição da UH (ex.: Apartamento de 1 quarto)."),
            new PlaceholderDescriptionReservas(OcupacaoMaxima, "Quantidade máxima de hóspedes permitidos para a UH."),
            new PlaceholderDescriptionReservas(VagaEstacionamento, "Informação sobre vagas de estacionamento."),
            new PlaceholderDescriptionReservas(HospedesLista, "Lista formatada dos hóspedes vinculados à reserva."),
            new PlaceholderDescriptionReservas(TermoCessao, "Conteúdo configurável do termo de cessão."),
            new PlaceholderDescriptionReservas(Observacoes, "Observações adicionais ou instruções específicas."),
            new PlaceholderDescriptionReservas(QuantidadePaxPorFaixaEtaria, "Quantidade de PAX por faixa etária."),
            new PlaceholderDescriptionReservas(LocalAtendimento, "Nome do local de atendimento ao cliente")
        });

    public static IReadOnlyCollection<PlaceholderDescriptionReservas> All => _all;
}

public record PlaceholderDescriptionReservas(string Key, string Description);

