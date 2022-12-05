using System.Text;

namespace Macronizer.Filters;

/// <summary>
/// Interface for filters working on a source text.
/// </summary>
public interface ITextFilter
{
    /// <summary>
    /// Gets or sets a value indicating whether this filter is enabled.
    /// </summary>
    bool IsEnabled { get; set; }

    /// <summary>
    /// Resets the internal state of this filter if any.
    /// </summary>
    void Reset();

    /// <summary>
    /// Gets the filter's state if any.
    /// </summary>
    /// <typeparam name="T">The type to cast the state to.</typeparam>
    /// <returns>State or null.</returns>
    T? GetState<T>() where T : class;

    /// <summary>
    /// Applies the filter to the specified text.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="context">An optional context object.</param>
    void Apply(StringBuilder text, object? context = null);
}
