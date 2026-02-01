namespace AccessCenterDomain.AccessCenter.Fractional
{
    public class FrCrcAtendimento : EntityBase
    {
        public virtual int? Filial { get; set; }
        public virtual string Codigo { get; set; }
        public virtual int? Cliente { get; set; }
        public virtual int? FrAtendimentoVenda { get; set; }
        public virtual DateTime? DataHoraInicio { get; set; }
        public virtual DateTime? DataHoraFim { get; set; }
        public virtual int? FrCrcTipoAtendimento { get; set; }
        public virtual int? FrSala { get; set; }
        public virtual string Status { get; set; } = "E";
        public virtual string Descricao { get; set; }
        public virtual string Observacao { get; set; }
        public virtual string ObservacaoCompleta { get; set; }
        public virtual int? GrupoEmpresa { get; set; }
        public virtual int? Empresa { get; set; }
        public virtual string IntegracaoId { get; set; } //TSE6766 SINTAX TSE+IDCONTATO_NO_TSE
        public virtual int? FrCrcMeioComunicacao { get; set; }
        public virtual int? FrCrcResultadoAtendimento { get; set; }

    }
}
