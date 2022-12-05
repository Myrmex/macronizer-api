using System.ComponentModel.DataAnnotations;

namespace MacronizerApi.Models
{
    /// <summary>
    /// A request for text macronization.
    /// </summary>
    public class MacronizerRequest : MacronizerOptions
    {
        /// <summary>
        /// The plain text to macronize.
        /// </summary>
        [Required]
        [MaxLength(50000)]
        public string? Text { get; set; }

        /// <summary>
        /// True to apply whitespace normalization before macronization.
        /// </summary>
        public bool NormalizeWS { get; set; }

        /// <summary>
        /// True to apply Mn-category Unicode characters precomposition
        /// before macronization.
        /// </summary>
        public bool PrecomposeMN { get; set; }

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

        /// <summary>
        /// Determines whether this request has any preprocessing filter.
        /// </summary>
        public bool HasPreFilters() => NormalizeWS || PrecomposeMN;

        /// <summary>
        /// Determines whether this request has any postprocessing filter.
        /// </summary>
        public bool HasPostFilters() => UnmarkedEscapeOpen != null ||
            UnmarkedEscapeClose != null ||
            AmbiguousEscapeOpen != null ||
            AmbiguousEscapeClose != null ||
            UnknownEscapeOpen != null ||
            UnknownEscapeClose != null;
    }
}
