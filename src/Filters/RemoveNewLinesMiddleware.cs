using System.Text.Json;

namespace SW_PortalProprietario.API.src.Filters
{
    public class RemoveNewLinesMiddleware
    {
        private readonly RequestDelegate _next;

        public RemoveNewLinesMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.ContentType != null && context.Request.ContentType.Contains("application/json"))
            {
                context.Request.EnableBuffering();

                using (var reader = new StreamReader(context.Request.Body, leaveOpen: true))
                {
                    var body = await reader.ReadToEndAsync();
                    body = body.Replace("\n", " ").Replace("\r", " ").Replace("\"", "\\\"");
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                    var objetoDeserializado = JsonSerializer.Deserialize<dynamic>(body, options);

                    // Serializa o dicionário novamente para JSON
                    var sanitizedBody = JsonSerializer.Serialize(objetoDeserializado);

                    var buffer = System.Text.Encoding.UTF8.GetBytes(sanitizedBody);
                    context.Request.Body = new MemoryStream(buffer);
                    context.Request.Body.Seek(0, SeekOrigin.Begin);
                }
            }

            await _next(context);
        }


    }
}
