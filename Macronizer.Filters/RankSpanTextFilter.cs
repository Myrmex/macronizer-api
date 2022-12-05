using System;
using System.Text;

namespace Macronizer.Filters;

/// <summary>
/// Filter all the span tags representing macronization result rankings
/// (ambiguous or unknown or just without attributes) by replacing them
/// as specified by options.
/// </summary>
public sealed class RankSpanTextFilter : TextFilter, ITextFilter
{
    public void Apply(StringBuilder text, object? context = null)
    {
        if (text is null) throw new ArgumentNullException(nameof(text));

        throw new NotImplementedException();
    }
}