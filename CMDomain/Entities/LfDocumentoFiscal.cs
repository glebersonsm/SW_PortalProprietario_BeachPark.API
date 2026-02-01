namespace CMDomain.Entities
{
    public class LfDocumentoFiscal : CMEntityBase
    {
        public virtual int? IdDocumento { get; set; }
        public virtual string? CodSituacao { get; set; } = "00";
        public virtual int? IdModulo { get; set; } = 3;
        public virtual int? IdModelo { get; set; } = 37;
        public virtual int? IdHotel { get; set; }
        public virtual string? NumDocumentoIni { get; set; }
        public virtual string? NumDocumentoFin { get; set; }
        public virtual string? Serie { get; set; } = "NFS";
        public virtual string? SubSerie { get; set; }
        public virtual DateTime? DataEmissao { get; set; }
        public virtual DateTime? DataMovimento { get; set; }
        public virtual string? FlgTipo { get; set; } = "T";
        public virtual decimal? VlrDocumento { get; set; }
        public virtual string? FlgBenFiscal { get; set; } = "N";
        public virtual string? IdRelativo { get; set; }
        public virtual string? Placa { get; set; }
        public virtual string? ChaveNfEletronica { get; set; }
        public virtual string? CodClasseConsumo { get; set; } = "E01";
        public virtual string? CodTipoLigacao { get; set; } = "1";
        public virtual string? CodGrupoTensao { get; set; } = "01";
        public virtual string? NumeroGuia { get; set; }
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }

    }
}
