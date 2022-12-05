using System;
using System.Diagnostics;
using System.Text;

namespace Macronizer.Filters;

/// <summary>
/// Whitespaces filter. This normalizes space/tab characters by
/// replacing them with a single space, and trimming the text at both
/// edges. It also normalizes CR+LF into LF only.
/// </summary>
public sealed class WhitespaceTextFilter : TextFilter, ITextFilter
{
    private static bool IsSpcOrTab(char c) => c == ' ' || c == '\t';

    /// <summary>
    /// Applies the filter to the specified text.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="context">Not used.</param>
    /// <exception cref="ArgumentNullException">text</exception>
    public void Apply(StringBuilder text, object? context = null)
    {
        if (text == null) throw new ArgumentNullException(nameof(text));

        // normalize CRLF into LF only
        text.Replace("\r\n", "\n");

        // reach last non WS
        int i = text.Length - 1;
        while (i > -1 && IsSpcOrTab(text[i])) i--;
        if (i == -1)
        {
            text.Clear();
            return;
        }
        if (i < text.Length - 1)
            text.Remove(i + 1, text.Length - (i + 1));

        // reach first non WS
        i = 0;
        while (i < text.Length && IsSpcOrTab(text[i])) i++;
        Debug.Assert(i < text.Length);
        if (i > 0) text.Remove(0, i);

        // normalize any other whitespace
        i = text.Length - 1;
        while (i > -1)
        {
            if (IsSpcOrTab(text[i]))
            {
                int right = i;
                text[i--] = ' ';
                while (i > -1 && IsSpcOrTab(text[i])) i--;
                if (right - i > 1) text.Remove(i + 1, right - i - 1);
            }
            else i--;
        }
    }
}
