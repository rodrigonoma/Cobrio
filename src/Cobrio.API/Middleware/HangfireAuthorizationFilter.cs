using Hangfire.Dashboard;

namespace Cobrio.API.Middleware;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        // Em desenvolvimento, permite acesso sem autenticação
        if (httpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
        {
            return true;
        }

        // Em produção, requer autenticação
        return httpContext.User.Identity?.IsAuthenticated ?? false;
    }
}
