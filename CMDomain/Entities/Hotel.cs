namespace CMDomain.Entities
{
    public class Hotel : CMEntityBase
    {
        public virtual int? IdHotel { get; set; }
        public virtual int? IdPessoa { get; set; }
        public virtual int? IdEmpresa { get; set; }
        public virtual int? IdUnidNegoc { get; set; }
        public virtual int? IdRegiaoHotel { get; set; }
        public virtual int? IdEmpCondominio { get; set; }
        public virtual int? IdRedeHotel { get; set; }
        public virtual string? Ativo { get; set; } = "S";
        public virtual string? FlgUsaNoSistema { get; set; } = "S";
        public virtual string? FlgOutrosHoteis { get; set; } = "N";
        public virtual string? NomeEmpreendimento { get; set; }
        public virtual string? FlgTimeSharing { get; set; } = "N";
        public virtual string? FlgSpa { get; set; } = "N";
    }
}
