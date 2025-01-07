using System.Text;

namespace FileServer;

internal sealed class BasicAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _username = "admin";  // Example username
    private readonly string _password = "password"; // Example password

    public BasicAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

        if (authHeader is not null && authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            var encodedCredentials = authHeader.Substring("Basic ".Length).Trim();
            var decodedCredentials = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));
            var credentials = decodedCredentials.Split(':');

            if (credentials.Length == 2 && credentials[0] == _username && credentials[1] == _password)
            {
                await _next(context).ConfigureAwait(false);
                return;
            }
        }

        // If authentication fails, return a 401 Unauthorized response
        context.Response.StatusCode = 401;
        context.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Example\"";
        await context.Response.WriteAsync("Unauthorized").ConfigureAwait(false);
    }
}
