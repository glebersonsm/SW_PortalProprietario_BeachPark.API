using SW_PortalProprietario.Application.Models;

namespace SW_PortalProprietario.Application.Models.TimeSharing
{
    public class HospedeInputModel
    {
        public Int64? Id { get; set; }
        public Int64? IdHospede { get; set; }
        public string? TipoHospede { get; set; }
        public string? IdTipoHospede { get; set; }
        public Int64? ClienteId { get; set; }
        public string? Principal { get; set; }
        public string? Nome { get; set; }
        public string? Cpf { get; set; }
        public DateTime? DataNascimento { get; set; }
        public string? Documento { get; set; }
        public string? TipoDocumento { get; set; }
        public int? TipoDocumentoId { get; set; }
        public string? Email { get; set; }
        public string? Sexo { get; set; }
        public string? DDI { get; set; }
        public string? DDD { get; set; }
        public string? Telefone { get; set; }
        public string? CodigoIbge { get; set; }
        public string? Logradouro { get; set; }
        public string? Numero { get; set; }
        public string? Bairro { get; set; }
        public string? CEP { get; set; }
        public int? CidadeId { get; set; }
        public string? CidadeNome { get; set; }
        public string? CidadeUf { get; set; }
        public string? SiglaEstado { get; set; }
        public int? Estrangeiro { get; set; } //0 = não, 1 = Sim
        public DateTime? DataCheckin { get; set; }
        public DateTime? DataCheckout { get; set; }

        public static explicit operator HospedeInputDto(HospedeInputModel model)
        {
            return new HospedeInputDto
            {
                Id = model.Id,
                IdHospede = model.IdHospede,
                TipoHospede = model.TipoHospede,
                ClienteId = model.ClienteId,
                Principal = model.Principal,
                Nome = model.Nome,
                Cpf = model.Cpf,
                DataNascimento = model.DataNascimento?.ToString("yyyy-MM-dd"),
                Documento = model.Documento,
                TipoDocumento = model.TipoDocumento,
                TipoDocumentoId = model.TipoDocumentoId,
                Email = model.Email,
                Sexo = model.Sexo,
                DDI = model.DDI,
                DDD = model.DDD,
                Telefone = model.Telefone,
                CodigoIbge = model.CodigoIbge,
                Logradouro = model.Logradouro,
                Numero = model.Numero,
                Bairro = model.Bairro,
                CEP = model.CEP,
                CidadeId = model.CidadeId,
                CidadeNome = model.CidadeNome,
                SiglaEstado = model.SiglaEstado,
                Estrangeiro = model.Estrangeiro,
                CheckIn = model.DataCheckin?.ToString("yyyy-MM-dd"),
                CheckOut = model.DataCheckout?.ToString("yyyy-MM-dd"),
                IdTipoHospede = model.IdTipoHospede,
                CidadeUf = model.CidadeUf
            };
        }
    }
}