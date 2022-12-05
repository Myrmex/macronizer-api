using Fusi.Text.Unicode;

namespace Macronizer.Filters;

/// <summary>
/// Text filter base.
/// </summary>
public abstract class TextFilter
{
    private static UniData? _ud;

    /// <summary>
    /// Gets or sets a value indicating whether this filter is enabled.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Gets a shared Unicode data helper.
    /// </summary>
    protected static UniData UniData
    {
        get
        {
            return _ud ??= new UniData();
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TextFilter"/> class.
    /// </summary>
    protected TextFilter()
    {
        IsEnabled = true;
    }

    /// <summary>
    /// Resets the internal state of this filter if any.
    /// Most filters have no state, so the base implementation just does
    /// nothing. Override this to add custom behavior.
    /// </summary>
    public virtual void Reset()
    {
    }

    /// <summary>
    /// Gets the internal state of this filter if any.
    /// The default implementation just returns default(T).
    /// </summary>
    /// <typeparam name="T">The type to cast the state to.</typeparam>
    /// <returns>State.</returns>
    public virtual T? GetState<T>() where T : class
    {
        return default;
    }
}
