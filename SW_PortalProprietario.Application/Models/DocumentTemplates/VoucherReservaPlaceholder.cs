using System.Collections.ObjectModel;

namespace SW_PortalProprietario.Application.Models.DocumentTemplates;

/// <summary>
/// Lista de chaves disponÃ­veis para montagem do template de voucher de reserva.
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
            new PlaceholderDescriptionReservas(NomeCliente, "Nome do cliente (proprietÃ¡rio principal)."),
            new PlaceholderDescriptionReservas(DocumentoCliente, "Documento (CPF/CNPJ) do cliente."),
            new PlaceholderDescriptionReservas(NomeCoCessionario, "Nome do co-cessionÃ¡rio."),
            new PlaceholderDescriptionReservas(DocumentoCoCessionario, "Documento do co-cessionÃ¡rio."),
            new PlaceholderDescriptionReservas(ContratoNumero, "CÃ³digo do contrato vinculado Ã  reserva."),
            new PlaceholderDescriptionReservas(ReservaNumero, "NÃºmero da reserva gerado pela TSE."),
            new PlaceholderDescriptionReservas(HotelNome, "Nome do hotel ou empreendimento."),
            new PlaceholderDescriptionReservas(CheckInData, "Data do check-in formatada (dd/MM/yyyy)."),
            new PlaceholderDescriptionReservas(CheckInHora, "HorÃ¡rio de inÃ­cio permitido para check-in."),
            new PlaceholderDescriptionReservas(CheckOutData, "Data do check-out formatada (dd/MM/yyyy)."),
            new PlaceholderDescriptionReservas(CheckOutHora, "HorÃ¡rio limite para check-out."),
            new PlaceholderDescriptionReservas(HospedePrincipalNome, "Nome do hÃ³spede principal informado na reserva."),
            new PlaceholderDescriptionReservas(HospedePrincipalDocumento, "Documento do hÃ³spede principal."),
            new PlaceholderDescriptionReservas(TipoUtilizacao, "Tipo de utilizaÃ§Ã£o (Uso PrÃ³prio, Uso Convidado, Intercambiadora)."),
            new PlaceholderDescriptionReservas(TipoDisponibilizacao, "DescriÃ§Ã£o complementar do tipo de utilizaÃ§Ã£o."),
            new PlaceholderDescriptionReservas(TipoApartamento, "DescriÃ§Ã£o da UH (ex.: Apartamento de 1 quarto)."),
            new PlaceholderDescriptionReservas(OcupacaoMaxima, "Quantidade mÃ¡xima de hÃ³spedes permitidos para a UH."),
            new PlaceholderDescriptionReservas(VagaEstacionamento, "InformaÃ§Ã£o sobre vagas de estacionamento."),
            new PlaceholderDescriptionReservas(HospedesLista, "Lista formatada dos hÃ³spedes vinculados Ã  reserva."),
            new PlaceholderDescriptionReservas(TermoCessao, "ConteÃºdo configurÃ¡vel do termo de cessÃ£o."),
            new PlaceholderDescriptionReservas(Observacoes, "ObservaÃ§Ãµes adicionais ou instruÃ§Ãµes especÃ­ficas."),
            new PlaceholderDescriptionReservas(QuantidadePaxPorFaixaEtaria, "Quantidade de PAX por faixa etÃ¡ria."),
            new PlaceholderDescriptionReservas(LocalAtendimento, "Nome do local de atendimento ao cliente")
        });

    public static IReadOnlyCollection<PlaceholderDescriptionReservas> All => _all;
}

public record PlaceholderDescriptionReservas(string Key, string Description);

