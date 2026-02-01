namespace AccessCenterDomain.AccessCenter.Fractional
{
    public class FrHotel : EntityBase
    {
        public virtual int? Filial { get; set; }
        public virtual string Codigo { get; set; }
        public virtual string Nome { get; set; }
        public virtual string NomePesquisa { get; set; }
        public virtual string Integracao { get; set; } = "N";
        public virtual string IdIntegracao { get; set; }
        public virtual string PermitirReservaNorDifSetDia { get; set; } = "S";
        public virtual string CobraTaxaUtilizacao { get; set; } = "N";
        public virtual int IdadeCrianca1Inicio { get; set; } = 0;
        public virtual int IdadeCrianca1Fim { get; set; } = 5;
        public virtual int IdadeCrianca2Inicio { get; set; } = 6;
        public virtual int IdadeCrianca2Fim { get; set; } = 10;

    }
}
