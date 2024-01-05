using System.ComponentModel.DataAnnotations;
using System.Net;
using Klinkby.CleanApi;
using Klinkby.CleanApi.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
///     Add middleware services to the container.
/// </summary>
public static class CleanApiServiceCollectionExtensions
{
    private const string DefaultApiVersion = "v1";

    /// <summary>
    ///     Add middleware services to the container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add the services to.</param>
    /// <param name="configureOptions">A delegate to configure the <see cref="ServiceHostOptions" />.</param>
    /// <returns>The <see cref="IServiceCollection" /> so that additional calls can be chained.</returns>
    public static IServiceCollection AddClean(this IServiceCollection services,
        Action<ServiceHostOptions>? configureOptions)
    {
        var serviceHostOptions = new ServiceHostOptions();
        if (configureOptions is not null) configureOptions(serviceHostOptions);

        // used in UseClean() to configure middleware
        services.AddSingleton(serviceHostOptions);

        // support CORS
        var addCors = serviceHostOptions.CorsOrigins.Any();
        if (addCors)
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    {
                        builder.WithOrigins(serviceHostOptions.CorsOrigins)
                            .WithMethods(
                                HttpMethods.Get,
                                HttpMethods.Post,
                                HttpMethods.Put,
                                HttpMethods.Delete);
                        if (serviceHostOptions.Authorization) builder.AllowCredentials();
                    });
            });

        // support jwt authN + authZ
        if (serviceHostOptions.Authorization)
        {
            // support JWT AuthN
            services.AddAuthentication()
                .AddJwtBearer();
            // support AuthZ
            services.AddAuthorization();
        }

        // support rate limiting
        if (serviceHostOptions.RateLimiter is not null)
        {
            var bucketOptions = serviceHostOptions.RateLimiter;
            var limiter = new ProblemDetailsRateLimiter(bucketOptions);
            services.AddRateLimiter(limiter.Configure);
        }

        // support output caching
        if (serviceHostOptions.Cache) services.AddOutputCache();

        // support liveliness monitoring
        services.AddHealthChecks()
            .AddCheck<MemoryHealthCheck>("Memory");

        // support RFC7808 error response
        services.AddProblemDetails(options =>
            options.CustomizeProblemDetails = ctx =>
                ctx.ProblemDetails.Instance = ctx.HttpContext.Request.GetEncodedPathAndQuery()
        );

        // endpoint discovery for OpenAPI
        services.AddEndpointsApiExplorer();

        // set OpenAPI metadata from configuration
        services.AddSwaggerGen(
            options =>
            {
                var info = serviceHostOptions.OpenApi;
                options.SwaggerDoc(info.Version ?? DefaultApiVersion, info);
            });

        // support TLS
        if (serviceHostOptions.Https)
        {
            // server upgrade insecure http requests
            services.AddHttpsRedirection(
                options => options.RedirectStatusCode = (int)HttpStatusCode.PermanentRedirect);
            // client also
            services.AddHsts(options =>
                options.MaxAge = TimeSpan.FromSeconds(SecurityHeaderValue.HstsMaxAge));
        }

        // support compression to optimize bandwidth
        services.AddResponseCompression(
            options => options.EnableForHttps = serviceHostOptions.Https);

        // support x-correlation-id for distributed tracing
        services.AddHttpContextAccessor();
        services.AddTransient<Correlation>();
        services.AddTransient<CorrelationDelegatingHandler>();

        return services;
    }

    /// <summary>
    ///     Add middleware services to the container.
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static WebApplication UseClean(this WebApplication app)
    {
        var serviceHostOptions = app.Services.GetRequiredService<ServiceHostOptions>();
        app.UseSwagger();
        if (app.Environment.IsDevelopment())
        {
            // show exception details on dev
            app.UseDeveloperExceptionPage();
        }
        else
        {
            // don't enforce https on local dev
            app.UseHsts();
        }

        // support CORS
        if (serviceHostOptions.CorsOrigins.Any()) app.UseCors();
        if (serviceHostOptions.Authorization)
        {
            // support AuthN 
            app.UseAuthentication();
            // support AuthZ
            app.UseAuthorization();
        }

        // support rate limiting
        if (serviceHostOptions.RateLimiter is not null) app.UseRateLimiter();

        // support output caching
        if (serviceHostOptions.Cache) app.UseOutputCache();

        // json serialize health checks
        app.UseJsonSerializedHealthChecks(serviceHostOptions.HealthPath);

        // support RFC7808 error response
        app.UseStatusCodePages();

        // support compression to optimize bandwidth
        app.UseResponseCompression();

        // support TLS upgrade
        if (serviceHostOptions.Https) app.UseHttpsRedirection();

        // harden http
        app.UseSecurityHeaders();

        // convert ValidationException to ProblemDetails response
        app.UseExceptionHandler(
            configure => { configure.Run(ValidationExceptionHandler); }
        );

        return app;
    }

    private static Task ValidationExceptionHandler(HttpContext context)
    {
        var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
        if (exceptionHandlerFeature?.Error is not ValidationException validationException) return Task.CompletedTask;

        var result = validationException.ValidationResult;
        var validationProblem =
            TypedResults.ValidationProblem(
                result.MemberNames.ToDictionary(k => k, v => new[] { result.ErrorMessage })!);

        return HttpValidationProblemDetailsSerializerContext
            .Default
            .WriteProblemDetailsJsonAsync(validationProblem.ProblemDetails, context.Response);
    }
}