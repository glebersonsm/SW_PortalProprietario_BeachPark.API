namespace SW_Utils.Auxiliar
{
    public class ParameterValueResult
    {
        public string? Key { get; set; }
        public string? FriendlyName { get; set; }

        public ParameterValueResult()
        {

        }

        public ParameterValueResult(string keyValue, string friendlyName)
        {
            Key = keyValue;
            FriendlyName = friendlyName;

        }

    }
}
