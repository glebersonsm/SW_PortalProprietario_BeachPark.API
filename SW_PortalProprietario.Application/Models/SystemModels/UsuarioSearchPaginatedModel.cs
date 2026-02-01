using SW_PortalProprietario.Domain.Enumns;

namespace SW_PortalProprietario.Application.Models.SystemModels
{
    public class UsuarioSearchPaginatedModel
    {
        public int? Id { get; set; }
        public string? Email { get; set; }
        public string? CpfCnpj { get; set; }
        public string? NomePessoa { get; set; }
        public bool? CarregarEmpresas { get; set; }
        public bool? CarregarPermissoes { get; set; }
        public bool? CarregarGruposDeUsuarios { get; set; }
        public bool? CarregarDadosPessoa { get; set; }
        public EnumSimNao? Admininistrador { get; set; }
        public EnumSimNao? GestorFinanceiro { get; set; }
        public EnumSimNao? GestorReservasAgendamentos { get; set; }
        public int? NumeroDaPagina { get; set; }
        public int? QuantidadeRegistrosRetornar { get; set; }



    }
}
