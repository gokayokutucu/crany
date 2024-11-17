using Serilog;
using Serilog.Context;

namespace Crany.Web.Api;

public class RequestLoggingMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context)
    {
        var userId = context.User.FindFirst("uid")?.Value ?? "Anonymous";
        
        using (LogContext.PushProperty("RequestId", context.TraceIdentifier))
        using (LogContext.PushProperty("UserId", userId))
        {
            Log.Information("Handling request for {Path}", context.Request.Path);
            await next(context);    
            Log.Information("Finished handling request for {Path}", context.Request.Path);
        }
    }
}