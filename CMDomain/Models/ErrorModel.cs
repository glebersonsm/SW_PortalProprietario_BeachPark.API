namespace CMDomain.Models
{
    public class ErrorModel
    {
        public ErrorModel(string mensagem)
        {
            Erros.Add(mensagem);
        }
        public ErrorModel(Exception err)
        {
            Erros.Add(err.Message);
            if (err.InnerException != null)
                Erros.Add(err.InnerException.Message);
            if (err.StackTrace != null) Erros.Add(err.StackTrace);
        }
        public List<string> Erros { get; set; } = new List<string>();
    }
}
