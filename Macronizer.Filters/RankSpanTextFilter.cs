using System;
using System.Text;
using System.Xml.Linq;

namespace Macronizer.Filters;

/// <summary>
/// Filter all the span tags representing macronization result rankings
/// (ambiguous or unknown or just without attributes) by replacing them
/// as specified by options.
/// </summary>
public sealed class RankSpanTextFilter : TextFilter, ITextFilter
{
    public const string LONGS = "āēīōūĀĒĪŌŪ";

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
                if (options.DropNonMacronEscapes &&
                    elem.Value.Length == 1 &&
                    !LONGS.Contains(elem.Value[0]))
                {
                    text.Append(elem.Value);
                    continue;
                }

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
}

public class RankSpanTextFilterOptions
{
    /// <summary>
    /// A value indicating whether to drop escapes referred to vowels not
    /// having a macron. When macronizer returns a marked word, all
    /// the vowels in it are wrapped in a span, which can be rendered here
    /// according to the values set for the escape properties of these options.
    /// So, a word like <c>Gallia</c> might come out marked as ambiguous,
    /// and having vowels <c>a</c>, <c>i</c>, and <c>ā</c> marked inside it;
    /// yet, while the marks have the purpose of locating vowels, it's only
    /// the <c>ā</c> with the macron which should be intended as ambiguous.
    /// </summary>
    public bool DropNonMacronEscapes { get; set; }

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
