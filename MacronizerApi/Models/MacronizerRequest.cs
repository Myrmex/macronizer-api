using System.ComponentModel.DataAnnotations;
using System.Text;

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
        /// This normalizes space/tab characters by replacing them with a single
        /// space, and trimming the text at both edges. It also normalizes CR+LF
        /// into LF only.
        /// </summary>
        public bool NormalizeWS { get; set; }

        /// <summary>
        /// True to apply Mn-category Unicode characters precomposition
        /// before macronization. This precomposes Unicode Mn-category
        /// characters with their letters wherever possible. Apply this filter
        /// when the input text has Mn-characters to avoid potential issues with
        /// macronization and other processes.
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

        private static StringBuilder AppendEscapePair(string? op, string? cl,
            StringBuilder sb)
        {
            sb.Append("<[").Append(op ?? "NUL")
              .Append("] >[")
              .Append(cl ?? "NUL")
              .Append(']');
            return sb;
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            StringBuilder sb = new();

            sb.Append("txt: ").Append(Text?.Length ?? 0);

            sb.Append("| opt:").Append(base.ToString());

            if (HasPreFilters())
            {
                sb.Append("| pre: ");
                if (NormalizeWS) sb.Append("Ws");
                if (PrecomposeMN) sb.Append("Mn");
            }
            if (HasPostFilters())
            {
                sb.Append("| post: ").Append("Unm=");
                AppendEscapePair(UnmarkedEscapeOpen, UnmarkedEscapeClose, sb);
                sb.Append(" Amb=");
                AppendEscapePair(AmbiguousEscapeOpen, AmbiguousEscapeClose, sb);
                sb.Append(" Unk=");
                AppendEscapePair(UnknownEscapeOpen, UnknownEscapeClose, sb);
            }

            return sb.ToString();
        }
    }
}
