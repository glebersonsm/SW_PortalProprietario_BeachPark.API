namespace EsolutionPortalDomain.Portal
{
    public class PoolCotaProprietario : EntityBasePortal
    {
        public virtual int? Pool { get; set; }
        public virtual int? CotaProprietario { get; set; }
        public virtual DateTime? DataInclusao { get; set; }
        public virtual DateTime? DataSaida { get; set; }
        public virtual int? MinimoSemanas { get; set; }
        public virtual int? MaximoSemanas { get; set; }
        public virtual int? UsuarioInclusao { get; set; }
        public virtual DateTime? DataHoraInclusao { get; set; }
        public virtual int? UsuarioSaida { get; set; }
        public virtual DateTime? DataHoraSaida { get; set; }
        public virtual int? UsuarioExclusao { get; set; }
        public virtual DateTime? DataHoraExclusao { get; set; }
    }
}
