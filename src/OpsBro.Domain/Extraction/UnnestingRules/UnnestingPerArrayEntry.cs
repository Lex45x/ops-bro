using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using NLog;

namespace OpsBro.Domain.Extraction.UnnestingRules
{
    public class UnnestingPerArrayEntryRule : UnnestingRule
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        /// <inheritdoc />
        public UnnestingPerArrayEntryRule(string path, string target)
            : base(path, target)
        {
        }

        /// <inheritdoc />
        public override UnnestingRuleType Type => UnnestingRuleType.PerArrayEntry;

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

            if (!(unnestingProperty is JArray unnestingArray))
            {
                logger.Warn("Property {property} for unnesting is not array!", Path);
                yield return payload;
                yield break;
            }
            
            foreach (var entry in unnestingArray)
            {
                var payloadClone = payload.DeepClone() as JObject;
                if (payloadClone.ContainsKey("unnest"))
                {
                    payloadClone["unnest"][Target] = entry;
                }
                else
                {
                    payloadClone["unnest"] = new JObject
                    {
                        [Target] = entry
                    };
                }

                yield return payloadClone;
            }
        }
    }
}