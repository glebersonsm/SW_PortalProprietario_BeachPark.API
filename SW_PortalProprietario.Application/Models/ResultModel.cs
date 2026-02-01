namespace SW_PortalProprietario.Application.Models
{
    public class ResultModel<T>
    {
        public ResultModel()
        {

        }
        public ResultModel(T? result)
        {
            Data = result;
        }

        private int _Status = 500;
        public int Status
        {
            get { return _Status; }
            set
            {
                if (value < 401)
                    Success = true;
                else Success = false;

                _Status = value;

            }
        }
        public bool Success { get; set; }
        public T? Data { get; set; }
        public Dictionary<string, object>? AdditionalData { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public string? Message { get; set; }
    }

    public class DataResult<T>
    {
        public T? Data { get; set; }
    }

}
