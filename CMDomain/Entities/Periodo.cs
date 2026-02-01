namespace CMDomain.Entities
{
    public class Periodo : CMEntityBase
    {
        public virtual int? PerExercicio { get; set; }
        public virtual int? IdPessoa { get; set; }
        public virtual int? PerNumero { get; set; }

        public virtual DateTime? PerDatIni { get; set; }
        public virtual DateTime? PerDatFim { get; set; }
        public virtual string? PerNome { get; set; }
        public virtual string? PerBloque { get; set; } = "N";
        public virtual string? PerBloInt { get; set; } = "N";
        public virtual string? PerAtuali { get; set; } = "N";
        public virtual int? IdUsuarioInclusao { get; set; }
        public virtual string? PerOutMoeda { get; set; } = "N";
        public virtual string? PerEspecial { get; set; } = "N";
        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }


        public override int GetHashCode()
        {
            return PerExercicio.GetHashCode() + IdPessoa.GetHashCode() + PerNumero.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            Periodo? cc = obj as Periodo;
            if (cc is null) return false;
            return cc.Equals(this);
        }
    }
}
