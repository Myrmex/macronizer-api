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
using Serilog;
using Asp.Versioning;

namespace MacronizerApi.Controllers;

/// <summary>
/// Macronizer.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Produces("application/json")]
public sealed class MacronizerController : Controller
{
    private sealed class FlaskResult
    {
        public string? Result { get; set; }
        public string? Error { get; set; }
        public bool Maius { get; set; }
        public bool Utov { get; set; }
        public bool Itoj { get; set; }
        public bool Ambigs { get; set; }
    }

    private readonly string _serviceUri;
    private readonly int _timeout;

    /// <summary>
    /// Initializes a new instance of the <see cref="MacronizerController"/> class.
    /// </summary>
    /// <param name="config">The configuration.</param>
    public MacronizerController(IConfiguration config)
    {
        _serviceUri = config.GetValue<string>("AlatiusMacronizerUri")
            ?? "http://localhost:51234/";
        if (!_serviceUri.EndsWith("/")) _serviceUri += "/";

        int n = config.GetValue<int>("MacronizerTimeout");
        _timeout = n < 1 ? 3 : n;
    }

    private HttpClient GetClient(string apiRootUri)
    {
        HttpClient client = new()
        {
            BaseAddress = new Uri(apiRootUri)
        };
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
        client.Timeout = TimeSpan.FromMinutes(_timeout);

        return client;
    }

    private static void ApplyPreFilters(MacronizerRequest request)
    {
        StringBuilder text = new(request.Text);
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
    [Produces("application/json")]
    [ProducesResponseType(200)]
    public async Task<MacronizerResponse> Macronize(
        [FromBody] MacronizerRequest request)
    {
        Log.Logger.Information(
            "Macronization request: {Request} at {Now} UTC from {IP}",
            request,
            DateTime.UtcNow,
            HttpContext.Connection.RemoteIpAddress);

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

        var c = new
        {
            text = request.Text,
            maius = request.Maius,
            utov = request.Utov,
            itoj = request.Itoj,
            ambigs = request.Ambiguous
        };

        JsonContent content = JsonContent.Create(c, options: jsonOptions);
        content.Headers.ContentType!.CharSet = "";
        using HttpResponseMessage response = await client.PostAsync(
            _serviceUri + "macronize", content);

        if (!response.IsSuccessStatusCode)
        {
            Log.Logger.Error("Macronization error: {Code} {Reason}",
                response.StatusCode, response.ReasonPhrase);

            return new MacronizerResponse(request)
            {
                Error = $"{response.StatusCode}: {response.ReasonPhrase}"
            };
        }

        // string dump = await response.Content.ReadAsStringAsync();

        FlaskResult result = (await response.Content.ReadFromJsonAsync<FlaskResult>
            (jsonOptions))!;
        if (result.Error != null)
        {
            return new MacronizerResponse(request)
            {
                Error = result.Error
            };
        }

        // apply postprocessing filters if any
        if (request.HasPostFilters() && string.IsNullOrEmpty(result.Error))
        {
            ITextFilter filter = new RankSpanTextFilter();
            StringBuilder text = new(result.Result);
            filter.Apply(text, new RankSpanTextFilterOptions
            {
                DropNonMacronEscapes = request.DropNonMacronEscapes,
                UnmarkedEscapeOpen = request.UnmarkedEscapeOpen,
                UnknownEscapeClose = request.UnknownEscapeClose,
                AmbiguousEscapeOpen = request.AmbiguousEscapeOpen,
                AmbiguousEscapeClose = request.AmbiguousEscapeClose,
                UnknownEscapeOpen = request.UnknownEscapeOpen,
                UnmarkedEscapeClose = request.UnmarkedEscapeClose,
            });
            result.Result = text.ToString();
        }

        return new MacronizerResponse(request)
        {
            Result = result.Result,
            Error = result.Error
        };
    }
}
