namespace AccessCenterDomain.AccessCenter
{
    public class Cliente : EntityBase
    {
        public virtual int? Empresa { get; set; }
        public virtual int? Filial { get; set; }
        public virtual int? FilialVinculada { get; set; }
        public virtual string? Codigo { get; set; }
        public virtual int? Pessoa { get; set; }
        public virtual string Status { get; set; } = "A";
        public virtual string ExigeOrdemCompra { get; set; } = "S";
        public virtual string RestringeTipoRecebimento { get; set; } = "S";
        public virtual string RestringeCondicaoVenda { get; set; } = "N";
        public virtual string PermiteVendaSemCartao { get; set; } = "N";
        public virtual string PossuiFazenda { get; set; } = "N";
        public virtual string? EmailNFe { get; set; }
        public virtual string TipoClienteClassificacao { get; set; } = "C";
        public virtual int? TipoClientePrioritario { get; set; }
        public virtual string RestringeNaturezaOperacao { get; set; } = "N";
        public virtual string ObrigaXmlNfe { get; set; } = "N";
        public virtual string PermiteLibSemLimPeloGer { get; set; } = "S";
        public virtual string ContribuinteIcms { get; set; } = "N";
        public virtual int? GrupoEmpresa { get; set; } = 1;
        public virtual string ConsumidorFinal { get; set; } = "S";
        public virtual string? CondominioUsuario { get; set; }
        public virtual string? CondominioSenha { get; set; }
    }
}
