using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Macronizer.Filters;

/// <summary>
/// Filter all the span tags representing macronization result rankings
/// (ambiguous or unknown or just without attributes) by replacing them
/// as specified by options.
/// </summary>
public sealed partial class RankSpanTextFilter : TextFilter, ITextFilter
{
    private static readonly Regex _spanRegex = SpanRegex();

    private static void ProcessClosingSpan(int i, StringBuilder text,
        string replace)
    {
        while (i < text.Length - 6)
        {
            if (text[i] == '<' && text[i + 1] == '/' && text[i + 2] == 's' &&
                text[i + 3] == 'p' && text[i + 4] == 'a' && text[i + 5] == 'n' &&
                text[i + 6] == '>')
            {
                break;
            }
            i++;
        }
        if (i > text.Length - 6) return;    // defensive
        text.Remove(i, 7);
        if (text.Length > 0) text.Insert(i, replace);
    }

    public void Apply(StringBuilder text, object? context = null)
    {
        if (text is null) throw new ArgumentNullException(nameof(text));
        if (context is not RankSpanTextFilterOptions options) return;

        string s = text.ToString();
        int repLen = 0;

        foreach (Match m in _spanRegex.Matches(s).Reverse())
        {
            switch (m.Groups["c"].Value)
            {
                case "ambig":
                    if (options?.AmbiguousEscapeOpen != null)
                    {
                        text.Remove(m.Index, m.Length);
                        if (options.AmbiguousEscapeOpen.Length > 0)
                        {
                            text.Insert(m.Index, options.AmbiguousEscapeOpen);
                            repLen = options.AmbiguousEscapeOpen.Length;
                        }
                        else
                        {
                            repLen = 0;
                        }
                    }
                    if (options?.AmbiguousEscapeClose != null)
                    {
                        ProcessClosingSpan(m.Index + repLen, text,
                            options.AmbiguousEscapeClose);
                    }
                    break;

                case "unknown":
                    if (options?.UnknownEscapeOpen != null)
                    {
                        text.Remove(m.Index, m.Length);
                        if (options.UnknownEscapeOpen.Length > 0)
                        {
                            text.Insert(m.Index, options.UnknownEscapeOpen);
                            repLen = options.UnknownEscapeOpen.Length;
                        }
                        else
                        {
                            repLen = 0;
                        }
                    }
                    if (options?.UnknownEscapeClose != null)
                    {
                        ProcessClosingSpan(m.Index + repLen, text,
                            options.UnknownEscapeClose);
                    }
                    break;

                default:
                    if (options?.UnmarkedEscapeOpen != null)
                    {
                        text.Remove(m.Index, m.Length);
                        if (options.UnmarkedEscapeOpen.Length > 0)
                        {
                            text.Insert(m.Index, options.UnmarkedEscapeOpen);
                            repLen = options.UnmarkedEscapeOpen.Length;
                        }
                        else
                        {
                            repLen = 0;
                        }
                    }
                    if (options?.UnmarkedEscapeClose != null)
                    {
                        ProcessClosingSpan(m.Index + repLen, text,
                            options.UnmarkedEscapeClose);
                    }
                    break;
            }
        }
    }

    [GeneratedRegex("<span(?:\\s+class=\"(?<c>[^\"]+)\")?>", RegexOptions.Compiled)]
    private static partial Regex SpanRegex();
}

public class RankSpanTextFilterOptions
{
    /// <summary>
    /// The optional opening escape to use for an unmarked form instead
    /// of the default <c>&lt;span&gt;</c>. If not specified,
    /// the default is preserved. If empty, the tag is removed.
    /// </summary>
    public string? UnmarkedEscapeOpen { get; set; }

    /// <summary>
    /// The optional closing escape to use for an unmarked form instead
    /// of the default <c>&lt;/span&gt;</c>. If not specified,
    /// the default is preserved. If empty, the tag is removed.
    /// </summary>
    public string? UnmarkedEscapeClose { get; set; }

    /// <summary>
    /// The optional opening escape to use for an ambiguous form instead
    /// of the default <c>&lt;span class="ambig"&gt;</c>. If not specified,
    /// the default is preserved. If empty, the tag is removed.
    /// </summary>
    public string? AmbiguousEscapeOpen { get; set; }

    /// <summary>
    /// The optional closing escape to use for an ambiguous form instead
    /// of the default <c>&lt;/span&gt;</c>. If not specified,
    /// the default is preserved. If empty, the tag is removed.
    /// </summary>
    public string? AmbiguousEscapeClose { get; set; }

    /// <summary>
    /// The optional opening escape to use for an unknown form instead
    /// of the default <c>&lt;span class="unknown"&gt;</c>. If not specified,
    /// the default is preserved. If empty, the tag is removed.
    /// </summary>
    public string? UnknownEscapeOpen { get; set; }

    /// <summary>
    /// The optional closing escape to use for an unknown form instead
    /// of the default <c>&lt;/span&gt;</c>. If not specified,
    /// the default is preserved. If empty, the tag is removed.
    /// </summary>
    public string? UnknownEscapeClose { get; set; }
}