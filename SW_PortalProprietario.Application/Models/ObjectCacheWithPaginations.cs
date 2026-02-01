namespace SW_PortalProprietario.Application.Models
{
    public class ObjectCacheWithPaginations<T>
    {
        public ObjectCacheWithPaginations()
        {

        }

        public ObjectCacheWithPaginations(List<T> itens)
        {
            ListObjects = itens ?? new List<T>();
        }
        public List<T>? ListObjects { get; set; }
        public int? NumeroDaPagina { get; set; }
        public int LastPageNumber { get; set; }

    }
}
