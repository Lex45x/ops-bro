using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace OpsBro.Domain.Extraction.UnnestingRules
{
    /// <summary>
    /// Allows to unnest listener call into multiple calls
    /// </summary>
    public class UnnestingRule
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// Represents unnesting type. Currently only <see cref="UnnestingRuleType.PerRegexMatch"/> is supported.
        /// </summary>
        public UnnestingRuleType Type { get; } = UnnestingRuleType.PerRegexMatch;
        /// <summary>
        /// Path to the request property to apply unnesting
        /// </summary>
        public string Path { get; }
        /// <summary>
        /// Regex patter to find all matches
        /// </summary>
        public string Pattern { get; }
        /// <summary>
        /// Target property name which will take value of each regex match in each unnesting result
        /// </summary>
        public string Target { get; }

        public UnnestingRule(string path, string pattern, string target)
        {
            Path = path;
            Pattern = pattern;
            Target = target;
        }

        /// <summary>
        /// Finds all matches for <see cref="Pattern"/> in <paramref name="payload"/> by path <see cref="Path"/> and return new copy of payload for each match.
        /// </summary>
        /// <param name="payload">Request payload</param>
        /// <returns>Copies of payload with each regex match written in <see cref="Target"/></returns>
        public IEnumerable<JObject> Unnest(JObject payload)
        {
            if (payload is null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            var unnestingProperty = payload.SelectToken(Path);

            if (unnestingProperty == null)
            {
                logger.Warn("Property {property} for unnesting was not found!", Path);
                yield return payload;
                yield break;
            }

            var matches = Regex.Matches(unnestingProperty.ToObject<string>(), Pattern);

            foreach (Match match in matches)
            {
                var payloadClone = payload.DeepClone() as JObject;
                if (payloadClone.ContainsKey("unnest"))
                {
                    payloadClone["unnest"][Target] = match.Value;
                }
                else
                {
                    payloadClone["unnest"] = new JObject()
                    {
                        [Target] = match.Value
                    };
                }

                yield return payloadClone;
            }
        }
    }
}
