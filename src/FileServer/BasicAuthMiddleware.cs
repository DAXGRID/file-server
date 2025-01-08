using System.Security.Claims;
using System.Text;

namespace FileServer;

internal sealed class BasicAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly Dictionary<string, string> _fileServerUsers;

    public BasicAuthMiddleware(
        RequestDelegate next,
        IReadOnlyCollection<FileServerUser> fileServerUsers)
    {
        _next = next;
        _fileServerUsers = fileServerUsers.ToDictionary(x => x.Username, x => x.Password);
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

            if (credentials.Length == 2 && _fileServerUsers.TryGetValue(credentials[0], out string? password) && credentials[1] == password)
            {
                var identity = new ClaimsIdentity(new List<Claim>
                {
                    new Claim(ClaimTypes.Name, credentials[0])
                }, "Custom");

                context.User = new ClaimsPrincipal(identity);

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
