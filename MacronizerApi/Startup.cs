using System.Text.Json;
using MessagingApi;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;
using System.Reflection;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Microsoft.AspNetCore.HttpOverrides;
using MacronizerApi.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.IO;
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Http;
using System.Threading.RateLimiting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder.Extensions;

namespace MacronizerApi;

/// <summary>
/// Startup.
/// </summary>
public sealed class Startup
{
    /// <summary>
    /// Gets the configuration.
    /// </summary>
    public IConfiguration Configuration { get; }

    /// <summary>
    /// Gets the host environment.
    /// </summary>
    public IHostEnvironment HostEnvironment { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Startup"/> class.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <param name="environment">The environment.</param>
    public Startup(IConfiguration configuration, IHostEnvironment environment)
    {
        Configuration = configuration;
        HostEnvironment = environment;
    }

    private void ConfigureOptionsServices(IServiceCollection services)
    {
        // configuration sections
        // https://andrewlock.net/adding-validation-to-strongly-typed-configuration-objects-in-asp-net-core/
        services.Configure<MessagingOptions>(Configuration.GetSection("Messaging"));
        services.Configure<DotNetMailerOptions>(Configuration.GetSection("Mailer"));

        // explicitly register the settings object by delegating to the IOptions object
        services.AddSingleton(resolver =>
            resolver.GetRequiredService<IOptions<MessagingOptions>>().Value);
        services.AddSingleton(resolver =>
            resolver.GetRequiredService<IOptions<DotNetMailerOptions>>().Value);
    }

    private static void ConfigureCorsServices(IServiceCollection services)
    {
        services.AddCors(o => o.AddPolicy("CorsPolicy", builder =>
        {
            builder.AllowAnyMethod()
                .AllowAnyHeader()
                .AllowAnyOrigin();
        }));
    }

    private static void ConfigureSwaggerServices(IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "API",
                Description = "Macronizer Service"
            });
            c.DescribeAllParametersInCamelCase();

            // include XML comments
            // (remember to check the build XML comments in the prj props)
            string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);
        });
    }

    private async Task NotifyLimitExceededToRecipients()
    {
        // mailer must be enabled
        if (!Configuration.GetSection("Mailer").GetValue<bool>("IsEnabled"))
            return;

        // there must be recipient(s)
        IConfigurationSection section = Configuration.GetSection("Recipients");
        if (!section.Exists()) return;

        // build message
        MessagingOptions msgOptions = new();
        Configuration.GetSection("Messaging").Bind(msgOptions);
        IMessageBuilderService messageBuilder = new FileMessageBuilderService(
            msgOptions,
            HostEnvironment);

        Message? message = messageBuilder.BuildMessage("test-message",
            new Dictionary<string, string>()
            {
                ["EventTime"] = DateTime.UtcNow.ToString()
            });
        if (message == null) return;

        // send it to recipients
        string[] recipients = section.AsEnumerable()
            .Where(p => !string.IsNullOrEmpty(p.Value))
            .Select(p => p.Value!).ToArray();

        DotNetMailerOptions mailerOptions = new();
        Configuration.GetSection("Mailer").Bind(msgOptions);
        IMailerService mailer = new DotNetMailerService(mailerOptions);

        foreach (string recipient in recipients)
        {
            Log.Logger.Information("Sending email message");
            await mailer.SendEmailAsync(
                recipient,
                "Test Recipient",
                message);
            Log.Logger.Information("Email message sent");
        }
    }

    private void ConfigureRateLimiterService(IServiceCollection services)
    {
        // nope if Disabled
        var limit = Configuration.GetSection("Limit");
        if (!limit.GetValue<bool>("IsDisabled")) return;

        // PermitLimit (10)
        int permit = limit.GetValue<int>("PermitLimit");
        if (permit < 1) permit = 10;

        // QueueLimit (0)
        int queue = limit.GetValue<int>("QueueLimit");

        // Window (00:01:00 = HH:MM:SS)
        string? windowText = limit.GetValue<string>("Window");
        TimeSpan window;
        if (!string.IsNullOrEmpty(windowText))
        {
            if (!TimeSpan.TryParse(windowText, out window))
                window = TimeSpan.FromMinutes(1);
        }
        else window = TimeSpan.FromMinutes(1);

        // https://blog.maartenballiauw.be/post/2022/09/26/aspnet-core-rate-limiting-middleware.html
        // default = 10 requests per minute, per authenticated username,
        // or hostname if not authenticated.
        services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>
            (httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.User.Identity?.Name
                        ?? httpContext.Request.Headers.Host.ToString(),
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = permit,
                        QueueLimit = queue,
                        Window = window
                    }));

            options.OnRejected = async (context, token) =>
            {
                // 429 too many requests
                context.HttpContext.Response.StatusCode = 429;

                // log
                Log.Logger.Warning("Rate limit exceeded");

                // send
                await NotifyLimitExceededToRecipients();

                // ret JSON with error
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter,
                    out var retryAfter))
                {
                    await context.HttpContext.Response.WriteAsync(
                        "{\"error\": " +
                        "\"Too many requests. Please try again after " +
                        $"{retryAfter.TotalMinutes} minute(s).\"" +
                        "}");
                }
                else
                {
                    await context.HttpContext.Response.WriteAsync(
                        "{\"error\": " +
                        "\"Too many requests. Please try again later.\"" +
                        "}");
                }
            };
        });
    }

    /// <summary>
    /// Configures the services.
    /// </summary>
    /// <param name="services">The services.</param>
    public void ConfigureServices(IServiceCollection services)
    {
        // rate limiter
        ConfigureRateLimiterService(services);

        // configuration
        ConfigureOptionsServices(services);

        // CORS (before MVC)
        ConfigureCorsServices(services);

        // base services
        services.AddControllers();

        // versioning
        services.AddApiVersioning(config => {
            config.DefaultApiVersion = new ApiVersion(1, 0);
            config.AssumeDefaultVersionWhenUnspecified = true;
            config.ReportApiVersions = true;
            config.ApiVersionReader = new HeaderApiVersionReader("x-version");
        });

        // camel-case JSON in response
        services.AddMvc()
            // https://docs.microsoft.com/en-us/aspnet/core/migration/22-to-30?view=aspnetcore-2.2&tabs=visual-studio#jsonnet-support
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy =
                    JsonNamingPolicy.CamelCase;
            });

        // messaging
        // you can use another mailer service here. In this case,
        // also change the types in ConfigureOptionsServices.
        services.AddTransient<IMailerService, DotNetMailerService>();
        services.AddTransient<IMessageBuilderService, FileMessageBuilderService>();

        // configuration
        services.AddSingleton(_ => Configuration);

        // swagger
        ConfigureSwaggerServices(services);

        // serilog
        // https://github.com/RehanSaeed/Serilog.Exceptions
        string? intervalName = Configuration.GetSection("Serilog")
            .GetValue<string>("RollingInterval");
        RollingInterval interval = string.IsNullOrEmpty(intervalName)
            ? RollingInterval.Day
            : Enum.Parse<RollingInterval>(intervalName);

        services.AddSingleton<Serilog.ILogger>(_ => new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.WithExceptionDetails()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File("macronizer-log.txt", rollingInterval: interval)
            .CreateLogger());
    }

    /// <summary>
    /// Configures the specified application.
    /// </summary>
    /// <param name="app">The application.</param>
    /// <param name="env">The environment.</param>
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-nginx?view=aspnetcore-2.2#configure-a-reverse-proxy-server
        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor
                | ForwardedHeaders.XForwardedProto
        });

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            // https://docs.microsoft.com/en-us/aspnet/core/security/enforcing-ssl?view=aspnetcore-5.0&tabs=visual-studio
            app.UseExceptionHandler("/Error");
            if (Configuration.GetValue<bool>("Server:UseHSTS"))
            {
                Console.WriteLine("HSTS: yes");
                app.UseHsts();
            }
            else
            {
                Console.WriteLine("HSTS: no");
            }
        }

        if (Configuration.GetValue<bool>("Server:UseHttpsRedirection"))
        {
            Console.WriteLine("HttpsRedirection: yes");
            app.UseHttpsRedirection();
        }
        else
        {
            Console.WriteLine("HttpsRedirection: no");
        }

        app.UseRouting();

        // CORS
        app.UseCors("CorsPolicy");
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints => endpoints.MapControllers());

        // Swagger
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            string? url = Configuration.GetValue<string>("Swagger:Endpoint");
            if (string.IsNullOrEmpty(url)) url = "v1/swagger.json";
            options.SwaggerEndpoint(url, "V1 Docs");
            options.DocumentTitle = "Macronizer Service API";
            options.HeadContent = "Welcome to the Macronizer Service API.";
        });
    }
}
