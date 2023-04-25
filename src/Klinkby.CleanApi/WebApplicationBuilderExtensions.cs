using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net;
using Klinkby.CleanApi;

namespace Microsoft.Extensions.DependencyInjection;

public static class IServiceCollectionExtensions
{
    private const string DefaultApiVersion = "v1";

    public static IServiceCollection AddClean(this IServiceCollection services, Action<ServiceHostOptions> configureOptions)
    {
        var serviceHostOptions = new ServiceHostOptions();
        configureOptions(serviceHostOptions);
        services.AddSingleton(serviceHostOptions); // used in UseClean()
        bool addCors = serviceHostOptions.CorsOrigins.Any();
        if (addCors)
        {
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    {
                        builder.WithOrigins(serviceHostOptions.CorsOrigins)
                               .WithMethods(HttpMethods.Get, HttpMethods.Post, HttpMethods.Put, HttpMethods.Delete);
                        if (serviceHostOptions.Authorization)
                        {
                            builder.AllowCredentials();
                        }
                    });
            });
        }

        // add middleware services to the container
        if (serviceHostOptions.Authorization)
        {
            services.AddAuthentication().AddJwtBearer(); // support JWT AuthN
            services.AddAuthorization();                 // support AuthZ
        }
        if (serviceHostOptions.RateLimiter is not null)
        {
            var bucketOptions = serviceHostOptions.RateLimiter;
            var limiter = new ProblemDetailsRateLimiter(bucketOptions);
            services.AddRateLimiter(limiter.Configure);
        }
        if (serviceHostOptions.Cache)
        {
            services.AddOutputCache();      // support output caching
        }
        services.AddHealthChecks()          // support liveliness monitoring
                .AddCheck<MemoryHealthCheck>("Memory");
        services.AddProblemDetails();       // support RFC7808 error response
        services.AddEndpointsApiExplorer(); // endpoint discovery for OpenAPI
        services.AddSwaggerGen(             // OpenAPI metadata from configuration
            options =>
            {
                var info = serviceHostOptions.OpenApi;
                options.SwaggerDoc(info?.Version ?? DefaultApiVersion, info);
            });
        if (serviceHostOptions.Https)
        {
            services.AddHttpsRedirection(       // upgrade insecure http requests
                options => options.RedirectStatusCode = (int)HttpStatusCode.PermanentRedirect);
            services.AddHsts(options =>         // client should always upgrade requests
                options.MaxAge = TimeSpan.FromSeconds(SecurityHeaderValue.HstsMaxAge));
        }
        services.AddResponseCompression(        // optimize bandwidth
            options => options.EnableForHttps = serviceHostOptions.Https);

        services.AddHttpContextAccessor();
        services.AddScoped(typeof(ICommandInvoker<>), typeof(CommandInvokerFactory<>));
        return services;
    }

    public static WebApplication UseClean(this WebApplication app)
    {
        var serviceHostOptions = app.Services.GetRequiredService<ServiceHostOptions>();
        app.UseSwagger();
        if (app.Environment.IsDevelopment())
        {
            app.UseSwaggerUI();             // only enable html api browser on dev
        }
        else
        {
            app.UseHsts();                  // don't enforce https on local dev
        }
        if (serviceHostOptions.CorsOrigins.Any())
        {
            app.UseCors();                  // support CORS
        }
        if (serviceHostOptions.Authorization)
        {
            app.UseAuthentication();        // support AuthN    
            app.UseAuthorization();         // support AuthZ
        }
        if (serviceHostOptions.RateLimiter is not null)
        {
            app.UseRateLimiter();
        }
        if (serviceHostOptions.Cache)
        {
            app.UseOutputCache();               // support output caching
        }
        app.UseJsonSerializedHealthChecks(serviceHostOptions.HealthPath);// json serialize health
        app.UseStatusCodePages();           // triggers problemdetails
        app.UseResponseCompression();
        if (serviceHostOptions.Https)
        {
            app.UseHttpsRedirection();
        }
        app.UseSecurityHeaders();           // harden http
        return app;
    }
}