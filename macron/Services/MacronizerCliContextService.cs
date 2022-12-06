namespace Macronizer.Cli.Services;

/// <summary>
/// CLI context service.
/// </summary>
public class MacronizerCliContextService
{
    private readonly MacronizerCliContextServiceConfig _config;

    public string ServiceUri => _config.ServiceUri!;

    /// <summary>
    /// Initializes a new instance of the <see cref="MacronizerCliContextService"/>
    /// class.
    /// </summary>
    /// <param name="config">The configuration.</param>
    public MacronizerCliContextService(MacronizerCliContextServiceConfig config)
    {
        _config = config;
    }
}

/// <summary>
/// Configuration for <see cref="MacronizerCliContextService"/>.
/// </summary>
public class MacronizerCliContextServiceConfig
{
    /// <summary>
    /// Gets or sets the service endpoint.
    /// </summary>
    public string? ServiceUri { get; set; }

    /// <summary>
    /// Gets or sets the local directory to use when loading resources
    /// from the local file system.
    /// </summary>
    public string? LocalDirectory { get; set; }
}