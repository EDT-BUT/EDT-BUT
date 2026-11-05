using System.Globalization;
using System.Net;
using System.Threading.RateLimiting;
using Edt.Api.Security;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;

namespace Edt.Api.Extensions;

public static class SecurityExtensions
{
    private const string ContentSecurityPolicy =
        "default-src 'self'; " +
        "script-src 'self'; " +
        "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; " +
        "font-src https://fonts.gstatic.com; " +
        "img-src 'self' data:; " +
        "connect-src 'self'; " +
        "object-src 'none'; " +
        "base-uri 'self'; " +
        "form-action 'self'; " +
        "frame-ancestors 'none'";

    public static IServiceCollection AddSecurity(this IServiceCollection services, IConfiguration configuration)
    {
        var knownProxies = configuration.GetSection("ReverseProxy:KnownProxies").Get<string[]>() ?? [];
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            foreach (var proxy in knownProxies)
            {
                options.KnownProxies.Add(IPAddress.Parse(proxy));
            }
        });

        var limits = configuration.GetSection(RateLimitingOptions.SectionName).Get<RateLimitingOptions>() ?? new();
        services.AddRateLimiter(options =>
        {
            options.AddPolicy(RateLimitPolicies.Api, context => PerClient(context, limits.ApiPermitsPerMinute));
            options.AddPolicy(RateLimitPolicies.RoomsStatus, context => PerClient(context, limits.RoomsStatusPermitsPerMinute));
            options.OnRejected = RejectAsync;
        });

        services.AddHsts(options => options.MaxAge = TimeSpan.FromDays(365));

        return services;
    }

    public static WebApplication UseSecurity(this WebApplication app)
    {
        app.UseForwardedHeaders();

        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
            app.UseHttpsRedirection();
        }

        app.Use(async (context, next) =>
        {
            var headers = context.Response.Headers;
            headers.XContentTypeOptions = "nosniff";
            headers.XFrameOptions = "DENY";
            headers["Referrer-Policy"] = "no-referrer";
            headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
            if (!context.Request.Path.StartsWithSegments("/swagger"))
            {
                headers.ContentSecurityPolicy = ContentSecurityPolicy;
            }

            await next(context);
        });

        app.UseRateLimiter();

        return app;
    }

    private static RateLimitPartition<string> PerClient(HttpContext context, int permitsPerMinute) =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "inconnu",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitsPerMinute,
                Window = TimeSpan.FromMinutes(1),
            });

    private static async ValueTask RejectAsync(OnRejectedContext context, CancellationToken cancellationToken)
    {
        var httpContext = context.HttpContext;
        httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            httpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);
        }

        await httpContext.RequestServices.GetRequiredService<IProblemDetailsService>().WriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails =
            {
                Status = StatusCodes.Status429TooManyRequests,
                Title = "Trop de requêtes",
                Detail = "Trop de requêtes en peu de temps. Réessaie dans une minute.",
            },
        });
    }
}
