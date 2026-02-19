namespace SW_PortalProprietario.Application.Attributes
{
    /// <summary>
    /// Atributo para marcar mÃ©todos/controllers que devem usar Saga Pattern
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class UseSagaAttribute : Attribute
    {
        /// <summary>
        /// Nome da operaÃ§Ã£o para identificaÃ§Ã£o nos logs
        /// </summary>
        public string OperationName { get; set; }

        /// <summary>
        /// Indica se deve falhar silenciosamente ou lanÃ§ar exceÃ§Ã£o
        /// </summary>
        public bool ThrowOnFailure { get; set; } = true;

        /// <summary>
        /// Timeout em segundos para a operaÃ§Ã£o
        /// </summary>
        public int TimeoutSeconds { get; set; } = 300;

        public UseSagaAttribute(string operationName)
        {
            OperationName = operationName ?? throw new ArgumentNullException(nameof(operationName));
        }
    }
}
