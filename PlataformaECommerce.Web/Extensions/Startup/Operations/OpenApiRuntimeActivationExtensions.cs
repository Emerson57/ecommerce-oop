using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PlataformaECommerce.Web.Authorization;
using PlataformaECommerce.Web.Configuration;
using PlataformaECommerce.Web.OpenApi;

namespace PlataformaECommerce.Web.Extensions.Startup;

/// <summary>
/// Activa la superficie OpenAPI del host web en tiempo de ejecución según el entorno actual.
/// </summary>
public static class OpenApiRuntimeActivationExtensions
{
    /// <summary>
    /// Activa Swagger y Swagger UI para capacidades operativas visibles en tiempo de ejecución.
    /// </summary>
    public static WebApplication UseOperationsOpenApiRuntime(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        WebOpenApiSecurityOptions openApiSecurityOptions = app.Services
            .GetRequiredService<IOptions<WebOpenApiSecurityOptions>>()
            .Value;

        if (!ShouldExposeOpenApi(app, openApiSecurityOptions))
        {
            return app;
        }

        if (openApiSecurityOptions.RequireAuthorizationOutsideDevelopment && !app.Environment.IsDevelopment())
        {
            app.UseWhen(
                static context => context.Request.Path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase),
                branch => branch.Use(async (context, next) =>
                {
                    bool jsonRequest = context.Request.Path.Value?.EndsWith(".json", StringComparison.OrdinalIgnoreCase) == true;
                    AuthenticateResult authenticationResult = await context
                        .AuthenticateAsync(AuthorizationPolicies.AdminCookieScheme)
                        .ConfigureAwait(false);

                    if (!authenticationResult.Succeeded || authenticationResult.Principal is null)
                    {
                        if (jsonRequest)
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            return;
                        }

                        await context.ChallengeAsync(AuthorizationPolicies.AdminCookieScheme).ConfigureAwait(false);
                        return;
                    }

                    AuthorizationResult authorizationResult = await context.RequestServices
                        .GetRequiredService<IAuthorizationService>()
                        .AuthorizeAsync(authenticationResult.Principal, openApiSecurityOptions.RequiredPolicy.Trim())
                        .ConfigureAwait(false);

                    if (!authorizationResult.Succeeded)
                    {
                        if (jsonRequest)
                        {
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            return;
                        }

                        await context.ForbidAsync(AuthorizationPolicies.AdminCookieScheme).ConfigureAwait(false);
                        return;
                    }

                    context.User = authenticationResult.Principal;
                    context.Response.Headers["X-Robots-Tag"] = "noindex, nofollow";
                    await next(context).ConfigureAwait(false);
                }));
        }

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint($"/swagger/{SwaggerGroups.Public}/swagger.json", "API Pública v1");
            options.SwaggerEndpoint($"/swagger/{SwaggerGroups.Admin}/swagger.json", "API Administrativa v1");
            options.DocumentTitle = "PlataformaECommerce Swagger";
            options.DisplayRequestDuration();
            options.DefaultModelsExpandDepth(-1);
        });

        return app;
    }

    private static bool ShouldExposeOpenApi(WebApplication app, WebOpenApiSecurityOptions options)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(options);

        if (app.Environment.IsDevelopment())
        {
            return options.EnabledInDevelopment;
        }

        if (app.Environment.IsQualityAssuranceLike())
        {
            return options.EnabledInQualityAssurance;
        }

        if (app.Environment.IsProduction())
        {
            return options.EnabledInProduction;
        }

        return false;
    }
}
