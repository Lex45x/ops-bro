namespace OpsBro.Domain.Extraction.UnnestingRules
{
    /// <summary>
    /// Represents different styles of the unnesting
    /// </summary>
    public enum UnnestingRuleType
    {
        /// <summary>
        /// Create new unnesting row for each regex entry in string token
        /// </summary>
        PerRegexMatch = 1
    }
}
