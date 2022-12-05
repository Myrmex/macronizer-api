using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using MacronizerApi.Models;
using System.Text;
using Macronizer.Filters;
using Microsoft.Extensions.Logging;

namespace MacronizerApi.Controllers;

/// <summary>
/// Macronizer.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Produces("application/json")]
public sealed class MacronizerController : Controller
{
    private class FlaskResult
    {
        public string? Result { get; set; }
        public string? Error { get; set; }
    }

    private readonly ILogger<MacronizerController> _logger;

    private readonly string _serviceUri;

    /// <summary>
    /// Initializes a new instance of the <see cref="MacronizerController"/> class.
    /// </summary>
    /// <param name="config">The configuration.</param>
    /// <param name="logger">The logger.</param>
    public MacronizerController(IConfiguration config,
        ILogger<MacronizerController> logger)
    {
        _serviceUri = config.GetValue<string>("AlatiusMacronizerUri")
            ?? "http://localhost:51234/";
        if (!_serviceUri.EndsWith("/")) _serviceUri += "/";
        _logger = logger;
    }

    private static HttpClient GetClient(string apiRootUri)
    {
        HttpClient client = new()
        {
            BaseAddress = new Uri(apiRootUri)
        };
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        return client;
    }

    private static void ApplyPreFilters(MacronizerRequest request)
    {
        StringBuilder text = new StringBuilder(request.Text);
        if (request.NormalizeWS)
        {
            ITextFilter filter = new WhitespaceTextFilter();
            filter.Apply(text);
        }
        if (request.PrecomposeMN)
        {
            ITextFilter filter = new MnComposerTextFilter();
            filter.Apply(text);
        }
        request.Text = text.ToString();
    }

    /// <summary>
    /// Macronize the specified text.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns>The result.</returns>
    [HttpPost("api/macronize")]
    [ProducesResponseType(200)]
    public async Task<MacronizerResult> Macronize(
        [FromBody] MacronizerRequest request)
    {
        _logger.LogInformation("Macronization request: {Request}", request);

        JsonSerializerOptions jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // apply preprocessing filters if any
        if (request.HasPreFilters()) ApplyPreFilters(request);

        HttpClient client = GetClient(_serviceUri);
        // we need this to remove charset from content-type
        // (charset UTF8 for JSON is redundant and the Flask API checks
        // for exact content type match in header)
        // https://gunnarpeipman.com/httpclient-remove-charset/
        // https://stackoverflow.com/questions/9254891/what-does-content-type-application-json-charset-utf-8-really-mean
        JsonContent content = JsonContent.Create(request, options: jsonOptions);
        content.Headers.ContentType!.CharSet = "";
        HttpResponseMessage response = await client.PostAsync(
            _serviceUri + "macronize", content);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Macronization error: {Code} {Reason}",
                response.StatusCode, response.ReasonPhrase);

            return new MacronizerResult(request)
            {
                Error = $"{response.StatusCode}: {response.ReasonPhrase}"
            };
        }

        FlaskResult result = (await response.Content.ReadFromJsonAsync<FlaskResult>
            (jsonOptions))!;

        // apply postprocessing filters if any
        if (request.HasPostFilters() && !string.IsNullOrEmpty(result.Error))
        {
            ITextFilter filter = new RankSpanTextFilter();
            StringBuilder text = new(result.Result);
            filter.Apply(text, new RankSpanTextFilterOptions
            {
                UnmarkedEscapeOpen = request.UnmarkedEscapeOpen,
                UnknownEscapeClose = request.UnknownEscapeClose,
                AmbiguousEscapeOpen = request.AmbiguousEscapeOpen,
                AmbiguousEscapeClose = request.AmbiguousEscapeClose,
                UnknownEscapeOpen = request.UnknownEscapeOpen,
                UnmarkedEscapeClose = request.UnmarkedEscapeClose,
            });
            result.Result = text.ToString();
        }

        return new MacronizerResult(request)
        {
            Result = result.Result,
            Error = result.Error
        };
    }
}
