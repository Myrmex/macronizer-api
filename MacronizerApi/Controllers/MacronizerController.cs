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

namespace MacronizerApi.Controllers;

/// <summary>
/// Macronizer.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Produces("application/json")]
public sealed class MacronizerController : Controller
{
    private readonly string _serviceUri;

    /// <summary>
    /// Initializes a new instance of the <see cref="MacronizerController"/> class.
    /// </summary>
    /// <param name="config">The configuration.</param>
    public MacronizerController(IConfiguration config)
    {
        _serviceUri = config.GetValue<string>("AlatiusMacronizerUri")
            ?? "http://localhost:51234/";
        if (!_serviceUri.EndsWith("/")) _serviceUri += "/";
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
            return new MacronizerResult(request)
            {
                Error = $"{response.StatusCode}: {response.ReasonPhrase}"
            };
        }

        // TODO filtering

        return await response.Content.ReadFromJsonAsync<MacronizerResult>
            (jsonOptions) ?? new MacronizerResult(request) { Result = "" };
    }
}
