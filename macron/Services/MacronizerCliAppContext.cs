using Fusi.Cli.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace Macronizer.Cli.Services;

/// <summary>
/// CLI app context.
/// </summary>
/// <seealso cref="CliAppContext" />
public class MacronizerCliAppContext : CliAppContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MacronizerCliAppContext"/>
    /// class.
    /// </summary>
    /// <param name="config">The configuration.</param>
    /// <param name="logger">The logger.</param>
    public MacronizerCliAppContext(IConfiguration? config, ILogger? logger)
        : base(config, logger)
    {
    }

    /// <summary>
    /// Gets the context service.
    /// </summary>
    public virtual MacronizerCliContextService GetContextService()
    {
        return new MacronizerCliContextService(
            new MacronizerCliContextServiceConfig
            {
                ServiceUri = Configuration!.GetValue<string>("ServiceUri")!,
                LocalDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                    "Assets")
            });
    }
}