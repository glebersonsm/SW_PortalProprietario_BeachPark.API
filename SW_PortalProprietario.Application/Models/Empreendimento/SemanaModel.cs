using EsolutionPortalDomain.ReservasApiModels.Hotel;

namespace SW_PortalProprietario.Application.Models.Empreendimento
{
    public class SemanaModel
    {
        public int? Id { get; set; }
        public int? PeriodoCotaDisponibilidadeId { get; set; }
        public int? InventarioId { get; set; }
        public string? InventarioPool { get; set; }
        public DateTime? DataInicial { get; set; }
        public DateTime? DataFinal { get; set; }
        public string? TipoSemana { get; set; }
        public int? CotaId { get; set; }
        public string? CotaNome { get; set; }
        public int? UhCondominioId { get; set; }
        public string? UhCondominioNumero { get; set; }
        public string? TipoDisponibilizacao { get; set; }
        public string? TipoDisponibilizacaoNome { get; set; }
        public string? TipoUtilizacao { get; set; }
        public string? ReservasVinculadas { get; set; }
        public string? NomeProprietario { get; set; }
        public string? DocumentoProprietario { get; set; }
        public string? Ano { get; set; }
        public List<ReservaModel>? Reservas { get; set; } = new List<ReservaModel>();
        public bool? PodeRetirarDoPool { get; set; }
        public bool? PodeLiberarParaPool { get; set; }
        public bool? PodeForcarAlteracao { get; set; }
        public int? Capacidade { get; set; }
        public bool? PossuiContratoSCP { get; set; }
        public string? IdIntercambiadora { get; set; }
        public string? PessoaTitular1Tipo { get; set; }
        public string? PessoaTitular1CPF { get; set; }
        public string? PessoaTitualar1CNPJ { get; set; }
        public string? PadraoDeCor { get; set; } = "Default";
        public string? Detalhes { get; set; }

    }
}
