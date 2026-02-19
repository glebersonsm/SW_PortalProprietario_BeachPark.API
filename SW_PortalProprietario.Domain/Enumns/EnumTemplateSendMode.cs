namespace SW_PortalProprietario.Domain.Enumns
{
    /// <summary>
    /// Modo de envio do template de comunicação
    /// </summary>
    public enum EnumTemplateSendMode
    {
        /// <summary>
        /// 1 - Apenas no corpo do email (HTML)
        /// </summary>
        BodyHtmlOnly = 1,
        
        /// <summary>
        /// 2 - Apenas como anexo (PDF)
        /// </summary>
        AttachmentOnly = 2,
        
        /// <summary>
        /// 3 - Corpo do email (HTML) + Anexo (PDF)
        /// </summary>
        BodyHtmlAndAttachment = 3
    }
}
