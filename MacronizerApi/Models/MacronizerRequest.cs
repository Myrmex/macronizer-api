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
}
