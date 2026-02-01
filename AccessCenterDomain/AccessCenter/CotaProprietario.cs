namespace AccessCenterDomain.AccessCenter
{
    public class CotaProprietario : EntityBase
    {
        public virtual int? Cota { get; set; }
        public virtual int? Proprietario { get; set; }
        public virtual int? Procurador { get; set; }
        public virtual DateTime? DataAquisicao { get; set; }

    }
}
