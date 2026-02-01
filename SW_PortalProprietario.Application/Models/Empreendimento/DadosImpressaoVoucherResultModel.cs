namespace SW_PortalProprietario.Application.Models.Empreendimento
{
    public class DadosImpressaoVoucherResultModel
    {
        public int? AgendamentoId { get; set; }
        public string? NumeroReserva { get; set; }
        public int? NumReserva { get; set; }
        public string? NomeCliente { get; set; }
        public string? DocumentoCliente { get; set; }
        public string? NomeHotel { get; set; }
        public string? HospedePrincipal { get; set; }
        public string? HospedePrincipalDocumento { get; set; }
        public string? TipoUtilizacao { get; set; }//Uso Próprio ou Uso Convidado ou Uso Intercambiadora
        public string? TipoDisponibilizacao { get; set; }//Uso Próprio ou Uso Convidado ou Uso Intercambiadora
        public string? TipoUso { get; set; }//Uso Próprio ou Uso Convidado ou Uso Intercambiadora
        public string? Contrato { get; set; }
        public string? Observacao { get; set; }
        public string? DataChegada { get; set; }
        public string? HoraChegada { get; set; }
        public string? DataPartida { get; set; }
        public string? LocalAtendimento { get; set; } = "Equipe My Mabu";
        public string? HoraPartida { get; set; }
        public string? Acomodacao { get; set; }
        public string? QuantidadePax { get; set; }
        public int? UhCondominioId { get; set; }
        public string? UhCondominioNumero { get; set; }
        public string? CotaNome { get; set; }
        public int? CotaPortalId { get; set; }
        public string? IdIntercambiadora { get; set; }

        public string? DocumentoCoCessionario { get; set; }
        public string? NomeCocessionario { get; set; }
        public string? HospedePrincipalNome { get; set; }
        public string? TipoApartamento { get; set; }
        public int? OcupacaoMaxima { get; set; }
        public string? VagaEstacionamento { get; set; }
        public string? TermoCessao { get; set; }
        public string? Observacoes { get; set; }
        public string? QuantidadePaxPorFaixaEtaria { get; set; }
        public List<VoucherHospedeModel> Hospedes { get; set; } = new();
        public int? QuantidadeAdulto { get; internal set; }
        public int? QuantidadeCrianca1 { get; internal set; }
        public int? QuantidadeCrianca2 { get; set; }
        public int? ClienteReservante { get; set; }
    }

    public class VoucherHospedeModel
    {
        public string? Nome { get; set; }
        public string? Documento { get; set; }
        public bool Principal { get; set; }
        public bool Proprietario { get; set; }
    }
}
