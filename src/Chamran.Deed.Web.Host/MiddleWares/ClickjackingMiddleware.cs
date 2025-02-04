using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Chamran.Deed.Web.MiddleWares
{
    public class ClickjackingMiddleware
    {
        private readonly RequestDelegate _next;

        public ClickjackingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.Response.OnStarting(() =>
            {
                context.Response.Headers["X-Frame-Options"] = "DENY";

                context.Response.Headers["Content-Security-Policy"] = "frame-ancestors 'self'";

                return Task.CompletedTask;
            });

            await _next(context);
        }
    }

}
