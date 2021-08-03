namespace OpsBro.Domain.Extraction.ExtractionRules
{
    /// <summary>
    /// Specify a way to extract a single event property from the request
    /// </summary>
    public enum ExtractionType
    {
        /// <summary>
        /// Extracts value as is
        /// </summary>
        Copy = 0,

        /// <summary>
        /// Extracts first regex match from the value
        /// </summary>
        FirstRegexMatch = 1
    }
}