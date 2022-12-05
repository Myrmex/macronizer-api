using System.Text;

namespace MacronizerApi.Models
{
    /// <summary>
    /// Options for macronization.
    /// </summary>
    public class MacronizerOptions
    {
        /// <summary>
        /// True to macronize capitalized words.
        /// </summary>
        public bool Maius { get; set; }

        /// <summary>
        /// True to convert U to V.
        /// </summary>
        public bool Utov { get; set; }

        /// <summary>
        /// True to convert I to J.
        /// </summary>
        public bool Itoj { get; set; }

        /// <summary>
        /// Mark ambiguous results.
        /// </summary>
        public bool Ambiguous { get; set; }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            StringBuilder sb = new();
            if (Ambiguous) sb.Append('A');
            if (Itoj) sb.Append('I');
            if (Maius) sb.Append('M');
            if (Utov) sb.Append('U');
            return sb.ToString();
        }
    }
}
