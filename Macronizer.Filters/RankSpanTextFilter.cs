using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Macronizer.Filters;

/// <summary>
/// Filter all the span tags representing macronization result rankings
/// (ambiguous or unknown or just without attributes) by replacing them
/// as specified by options.
/// </summary>
public sealed partial class RankSpanTextFilter : TextFilter, ITextFilter
{
    private static void ProcessSpan(XElement span, StringBuilder text,
        RankSpanTextFilterOptions options)
    {
        foreach (XNode child in span.Nodes())
        {
            if (child is XText txt)
            {
                text.Append(txt.Value);
                continue;
            }
            if (child is XElement elem)
            {
                switch (span.Attribute("class")?.Value)
                {
                    case "ambig":
                        if (!string.IsNullOrEmpty(options.AmbiguousEscapeOpen))
                            text.Append(options.AmbiguousEscapeOpen);
                        text.Append(elem.Value);
                        if (!string.IsNullOrEmpty(options.AmbiguousEscapeClose))
                            text.Append(options.AmbiguousEscapeClose);
                        break;
                    case "unknown":
                        if (!string.IsNullOrEmpty(options.UnknownEscapeOpen))
                            text.Append(options.UnknownEscapeOpen);
                        text.Append(elem.Value);
                        if (!string.IsNullOrEmpty(options.UnknownEscapeClose))
                            text.Append(options.UnknownEscapeClose);
                        break;
                    default:
                        if (!string.IsNullOrEmpty(options.UnmarkedEscapeOpen))
                            text.Append(options.UnmarkedEscapeOpen);
                        text.Append(elem.Value);
                        if (!string.IsNullOrEmpty(options.UnmarkedEscapeClose))
                            text.Append(options.UnmarkedEscapeClose);
                        break;
                }
            }
        }
    }

    public void Apply(StringBuilder text, object? context = null)
    {
        if (text is null) throw new ArgumentNullException(nameof(text));

        if (context is not RankSpanTextFilterOptions options) return;

        XElement root = XElement.Parse($"<x>{text}</x>",
            LoadOptions.PreserveWhitespace);

        text.Clear();
        foreach (XNode node in root.Nodes())
        {
            if (node is XText txt)
            {
                text.Append(txt.Value);
                continue;
            }
            if (node is XElement elem) ProcessSpan(elem, text, options);
        }
    }

    [GeneratedRegex("<span(?:\\s+class=\"(?<c>[^\"]+)\")?>", RegexOptions.Compiled)]
    private static partial Regex SpanRegex();
}

public class RankSpanTextFilterOptions
{
    /// <summary>
    /// The optional opening escape to use for an unmarked-form vowel.
    /// </summary>
    public string? UnmarkedEscapeOpen { get; set; }

    /// <summary>
    /// The optional closing escape to use for an unmarked-form vowel.
    /// </summary>
    public string? UnmarkedEscapeClose { get; set; }

    /// <summary>
    /// The optional opening escape to use for an ambiguous-form vowel.
    /// </summary>
    public string? AmbiguousEscapeOpen { get; set; }

    /// <summary>
    /// The optional closing escape to use for an ambiguous-form vowel.
    /// </summary>
    public string? AmbiguousEscapeClose { get; set; }

    /// <summary>
    /// The optional opening escape to use for an unknown-form vowel.
    /// </summary>
    public string? UnknownEscapeOpen { get; set; }

    /// <summary>
    /// The optional closing escape to use for an unknown-form vowel.
    /// </summary>
    public string? UnknownEscapeClose { get; set; }
}
