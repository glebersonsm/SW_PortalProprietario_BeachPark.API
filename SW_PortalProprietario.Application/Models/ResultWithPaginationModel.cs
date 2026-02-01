namespace SW_PortalProprietario.Application.Models
{
    public class ResultWithPaginationModel<T>
        : ResultModel<T> where T : class
    {
        public ResultWithPaginationModel()
        {

        }
        public ResultWithPaginationModel(T? result)
        {
            Data = result;
        }
        public int PageNumber { get; set; }
        public int LastPageNumber { get; set; }
    }


}
