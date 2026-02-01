namespace AccessCenterDomain.AccessCenter.Fractional
{
    public class CotaAccessCenterModel
    {
        public int CotaAcId { get; set; }
        public int CotaId { get; set; }
        public int? UhCondominio { get; set; }
        public string? CotaNome { get; set; }
        public string? CotaCodigo { get; set; }
        public string? GrupoCotaNome { get; set; }
        public int? CotaProprietarioId { get; set; }
        public int? ProprietarioId { get; set; }
        public string? ProprietarioNome { get; set; }
        public string? Produto { get; set; }
        public string? Numero { get; set; }
        public string? TagCota { get; set; } //<Importado=S>|<PORTAL=>
        public int? GrupoCotaTipoCota { get; set; }
        public int? Imovel { get; set; }
        public string? ImovelNumero { get; set; }
        public string? NumeroImovel { get; set; }
        public string? TagImovel { get; set; }
        public int? ImovelBloco { get; set; }
        public string? CodigoBloco { get; set; }
        public string? NomeBloco { get; set; }
        public int? Proprietario { get; set; }
        public string? CpfProprietario { get; set; }
        public string? CnpjProprietario { get; set; }
        public string? EmailProprietario { get; set; }
        public int? Procurador { get; set; }
        public string? StatusCota { get; set; } = "D";
        public string? CotaBloqueada { get; set; }
        public int? FrAtendimentoVenda { get; set; }
        public int? CategoriaCota { get; set; }
        public string? CodigoCategoriaCota { get; set; }
        public string? NomeCategoriaCota { get; set; }
        public string? GrupoCotaTipoCotaCodigo { get; set; }
        public string? GrupoCotaTipoCotaNome { get; set; }
        public int? TipoCota { get; set; }
        public string? TipoCotaCodigo { get; set; }
        public string? TipoCotaNome { get; set; }
        public string? GrupoCotaCodigo { get; set; }
        public int? GrupoCota { get; set; }
        public string? CodigoGrupoCota { get; set; }
        public string? NomeGrupoCota { get; set; }
        public DateTime? DataAquisicao { get; set; }
        public int? ContratoTSEId { get; set; }
        public string? CodigoNumerico { get; set; }
        public string? AndarCodigo { get; set; }
        public string? AndarNome { get; set; }
        public int? PessoaProviderId { get; set; }
        public string? ProprietarioRG { get; set; }
        public string? ProprietarioCPF_CNPJ { get; set; }
        public int? Capacidade { get; set; } = 0;
        public string? LogradouroSocio { get; set; }
        public string? BairroSocio { get; set; }
        public string? CidadeSocio { get; set; }
        public string? UfCidadeSocio { get; set; }
        public string? CepEnderecoSocio { get; set; }
        public string? TelefoneFixo { get; set; }
        public string? TelefoneCelular { get; set; }
        public string? DadosBancariosRecebimentoRendimentos { get; set; }
        public string? Empreendimento { get; set; }
        public int? EmpresaAcId { get; set; }
        public int? EmpresaPortalId { get; set; }
        public string? IdIntercambiadora { get; set; }
        public string? NumeroContrato { get; set; }
        public string? PadraoDeCor { get; set; } = "Default";
    }
}
