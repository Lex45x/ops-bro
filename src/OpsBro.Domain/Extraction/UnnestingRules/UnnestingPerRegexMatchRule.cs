using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using NLog;

namespace OpsBro.Domain.Extraction.UnnestingRules
{
    public class UnnestingPerRegexMatchRule : UnnestingRule
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        /// <inheritdoc />
        public UnnestingPerRegexMatchRule(string path, string pattern, string target)
            : base(path, target)
        {
            Pattern = pattern;
        }

        /// <summary>
        /// Regex patter to find all matches
        /// </summary>
        public string Pattern { get; }

        /// <inheritdoc />
        public override UnnestingRuleType Type => UnnestingRuleType.PerRegexMatch;

        /// <inheritdoc />
        public override IEnumerable<JObject> Unnest(JObject payload)
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
                    payloadClone["unnest"] = new JObject
                    {
                        [Target] = match.Value
                    };
                }

                yield return payloadClone;
            }
        }
    }
}