namespace SW_PortalProprietario.Application.Models.Proprietario
{
    public class CotaPeriodoModel
    {
        public int Id { get; set; }
        public DateTime? DataHoraCriacao { get; set; }
        public int? UsuarioCriacao { get; set; }
        public int? Cota { get; set; }
        public string? NumeroImovel { get; set; }
        public string? Fracao { get; set; }
        public int? Proprietario { get; set; }
        public string? CodigoProprietario { get; set; }
        public string? NomeProprietario { get; set; }
        public DateTime? DataInicial { get; set; }
        public DateTime? DataFinal { get; set; }
        public string? OpcaoUso { get; set; } //P = Pool, U = Uso proprietário, I = Intercâmbio
        public int? Pool { get; set; }
        public string? CodigoPool { get; set; }
        public string? NomePool { get; set; }
        public bool? NoPoolHoje { get; set; }
        public string? Email { get; set; }

        public string OpcaoUsoNormalizada
        {
            get
            {
                string strRetorno = "Pool";
                if (OpcaoUso == "U")
                    strRetorno = "Uso do proprietário";
                else if (OpcaoUso == "I")
                    strRetorno = "Intercâmbio";
                return strRetorno;
            }
        }

    }
}
