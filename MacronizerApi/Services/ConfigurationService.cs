using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;

namespace MacronizerApi.Services;

internal sealed class ConfigurationService(IWebHostEnvironment env)
{
    private readonly IWebHostEnvironment _env =
        env ?? throw new ArgumentNullException(nameof(env));
    private IConfiguration? _configuration;

    public IConfiguration Configuration
    {
        get
        {
            _configuration ??= new ConfigurationBuilder()
                .AddJsonFile("appsettings.json",
                    optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{_env.EnvironmentName}.json",
                    optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            return _configuration;
        }
    }
}
