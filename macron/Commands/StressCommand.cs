using Fusi.Cli;
using Fusi.Cli.Commands;
using Macronizer.Cli.Services;
using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Macronizer.Cli.Commands;

internal sealed class StressCommand : ICommand
{
    private readonly StressCommandOptions _options;

    private StressCommand(StressCommandOptions options)
    {
        _options = options;
    }

    private static int SafeParseInt(string s, int fallback)
    {
        if (string.IsNullOrEmpty(s)) return fallback;
        return int.TryParse(s, CultureInfo.InvariantCulture, out int n)
            ? n : fallback;
    }

    public static void Configure(CommandLineApplication app,
        ICliAppContext context)
    {
        app.Description = "Stress-test macronizer API to check for rate limiting.";
        app.HelpOption("-?|-h|--help");

        CommandOption textOption = app.Option("-t|--text",
            "The text to macronize", CommandOptionType.SingleValue);
        CommandOption countOption = app.Option("-c|--count",
            "The count of requests", CommandOptionType.SingleValue);
        CommandOption timeoutOption = app.Option("-m|--timeout",
            "The request timeout in minutes", CommandOptionType.SingleValue);
        CommandOption parallelOption = app.Option("-p|--parallel",
            "Parallelize requests", CommandOptionType.NoValue);

        app.OnExecute(() =>
        {
            context.Command = new StressCommand(
                new StressCommandOptions(context)
                {
                    Text = textOption.Value() ??
                        "Gallia est omnis divisa in partes tres, quarum unam incolunt " +
                        "Belgae, aliam Aquitani, tertiam qui ipsorum lingua Celtae, " +
                        "nostra Galli appellantur.",
                    Timeout = SafeParseInt(timeoutOption.Value(), 5),
                    Count = SafeParseInt(countOption.Value(), 20),
                    IsParallelized = parallelOption.HasValue()
                });
            return 0;
        });
    }

    private HttpClient GetHttpClient(string uri)
    {
        HttpClient client = new HttpClient
        {
            BaseAddress = new Uri(uri)
        };

        if (_options.Timeout > 0)
            client.Timeout = TimeSpan.FromMinutes(_options.Timeout);

        return client;
    }

    private async Task DoParallelRequests(string text, int count)
    {
        MacronizerCliContextService service = _options.Context.GetContextService();

        List<HttpClient> clients = new(count);
        for (int i = 0; i < count; i++)
        {
            clients.Add(GetHttpClient(service.ServiceUri));
        }

        await Task.WhenAll(clients.Select(async c =>
        {
            int i = clients.IndexOf(c);
            ColorConsole.WriteInfo($"- request #{i + 1:000} ...");

            await c.PostAsJsonAsync("macronize", new
            {
                text
            });

            ColorConsole.WriteInfo($"- request #{i + 1:000} completed");
        }));
    }

    private async Task DoChainedRequests(string text, int count)
    {
        MacronizerCliContextService service = _options.Context.GetContextService();

        HttpClient client = GetHttpClient(service.ServiceUri);

        for (int n = 1; n <= count; n++)
        {
            ColorConsole.WriteInfo($"- request #{n:000} ...");

            await client.PostAsJsonAsync("macronize", new
            {
                text
            });

            ColorConsole.WriteInfo($"- request #{n:000} completed");
        }
    }

    public async Task Run()
    {
        ColorConsole.WriteWrappedHeader("Stress",
            headerColor: ConsoleColor.Green);
        Console.WriteLine($"Text length: {_options.Text.Length}");
        Console.WriteLine($"Count: {_options.Count}");
        Console.WriteLine($"Parallel: {(_options.IsParallelized ? "yes" : "no")}\n");

        if (_options.IsParallelized)
        {
            await DoParallelRequests(_options.Text, _options.Count);
        }
        else
        {
            await DoChainedRequests(_options.Text, _options.Count);
        }

        ColorConsole.WriteInfo("All done.");
    }
}

internal class StressCommandOptions :
    CommandOptions<MacronizerCliAppContext>
{
    public string Text { get; set; }
    public int Count { get; set; }
    public int Timeout { get; set; }
    public bool IsParallelized { get; set; }

    public StressCommandOptions(ICliAppContext options)
        : base((MacronizerCliAppContext)options)
    {
        Text = "";
    }
}
