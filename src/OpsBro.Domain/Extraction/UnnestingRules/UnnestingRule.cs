using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text;

namespace OpsBro.Domain.Extraction.UnnestingRules
{
    /// <summary>
    /// Allows to unnest listener call into multiple calls
    /// </summary>
    public abstract class UnnestingRule
    {
        /// <summary>
        /// Represents unnesting type. Currently only <see cref="UnnestingRuleType.PerRegexMatch"/> is supported.
        /// </summary>
        public abstract UnnestingRuleType Type { get; }

        /// <summary>
        /// Path to the request property to apply unnesting
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Target property name which will take value of each regex match in each unnesting result
        /// </summary>
        public string Target { get; }

        protected UnnestingRule(string path, string target)
        {
            Path = path;
            Target = target;
        }

        /// <summary>
        /// Finds all matches for <see cref="Pattern"/> in <paramref name="payload"/> by path <see cref="Path"/> and return new copy of payload for each match.
        /// </summary>
        /// <param name="payload">Request payload</param>
        /// <returns>Copies of payload with each regex match written in <see cref="Target"/></returns>
        public abstract IEnumerable<JObject> Unnest(JObject payload);
    }
}