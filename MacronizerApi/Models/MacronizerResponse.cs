namespace MacronizerApi.Models
{
    /// <summary>
    /// Macronizer result.
    /// </summary>
    /// <seealso cref="MacronizerOptions" />
    public class MacronizerResponse : MacronizerOptions
    {
        /// <summary>
        /// The macronized text.
        /// </summary>
        public string? Result { get; set; }

        /// <summary>
        /// The optional error message.
        /// </summary>
        public string? Error { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MacronizerResponse"/>
        /// class.
        /// </summary>
        /// <param name="options">The options.</param>
        public MacronizerResponse(MacronizerOptions options)
        {
            Maius = options.Maius;
            Utov = options.Utov;
            Itoj= options.Itoj;
            Ambiguous= options.Ambiguous;
        }
    }
}
