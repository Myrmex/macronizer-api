using System;
using System.Text;

namespace Macronizer.Filters;

/// <summary>
/// Mn-category precomposer filter. This precomposes Unicode Mn-category
/// characters with their letters wherever possible. Apply this filter when
/// the input text has Mn-characters to avoid potential issues with
/// macronization and other processes.
/// </summary>
/// <remarks>This filter just wraps a <see cref="MnComposer"/>.</remarks>
/// <seealso cref="ITextFilter" />
public sealed class MnComposerTextFilter : TextFilter, ITextFilter
{
    private readonly MnComposer _composer;

    /// <summary>
    /// Initializes a new instance of the <see cref="MnComposerTextFilter"/>
    /// class.
    /// </summary>
    public MnComposerTextFilter()
    {
        _composer = new MnComposer(UniData);
    }

    /// <summary>
    /// Applies the filter to the specified text.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="context">Not used.</param>
    /// <exception cref="ArgumentNullException">text</exception>
    public void Apply(StringBuilder text, object? context = null)
    {
        if (text == null)
            throw new ArgumentNullException(nameof(text));

        string result = _composer.Compose(text.ToString(), '\0');
        text.Clear();
        text.Append(result);
    }
}
