using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FluentValidation;
using Newtonsoft.Json.Linq;
using OpsBro.Domain.Events;
using OpsBro.Domain.Extraction.Rules;

namespace OpsBro.Domain.Extraction
{
    /// <summary>
    /// Define rules to extract an event from payload
    /// </summary>
    public class Extractor
    {
        public Extractor(string name, string @event, ICollection<ExtractionRule> extractionRules,
            ICollection<ValidationRule> validationRules)
        {
            Name = name;
            Event = @event;
            ExtractionRules = extractionRules;
            ValidationRules = validationRules;
            validator = CreateValidator();
        }

        private InlineValidator<JObject> CreateValidator()
        {
            var inlineValidator = new InlineValidator<JObject>
            {
                CascadeMode = CascadeMode.StopOnFirstFailure
            };

            foreach (var validationRule in ValidationRules)
            {
                switch (validationRule.Operator)
                {
                    case ValidationOperator.Equals:
                        inlineValidator.RuleFor(o => o.SelectToken(validationRule.Path))
                            .Must(token =>
                            {
                                var comparisonValue = JToken.FromObject(validationRule.Value ?? JValue.CreateNull());
                                var tokensAreEqual = JToken.DeepEquals(token, comparisonValue);

                                return tokensAreEqual;
                            });
                        break;
                    case ValidationOperator.NotEquals:
                        inlineValidator.RuleFor(o => o.SelectToken(validationRule.Path))
                            .Must(token =>
                            {
                                var comparisonValue = JToken.FromObject(validationRule.Value ?? JValue.CreateNull());
                                var tokensAreEqual = JToken.DeepEquals(token, comparisonValue);

                                return !tokensAreEqual;
                            });
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return inlineValidator;
        }

        /// <summary>
        /// The name of a Extractor
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Event that will be extracted as a result
        /// </summary>
        public string Event { get; }

        /// <summary>
        /// Set of rules to extract an event
        /// </summary>
        public ICollection<ExtractionRule> ExtractionRules { get; }

        /// <summary>
        /// ValidationRules that help to ensure that event can be extracted
        /// </summary>
        public ICollection<ValidationRule> ValidationRules { get; }

        private readonly IValidator<JObject> validator;

        /// <summary>
        /// Makes an attempt to extract event from payload
        /// </summary>
        /// <param name="payload">Payload to extract from</param>
        /// <param name="extractedEvent">Extracted event</param>
        /// <returns>Status of extraction. True when <paramref name="extractedEvent"/> created and initialized. </returns>
        public bool TryExtract(JObject payload, [MaybeNullWhen(returnValue: false)] out Event extractedEvent)
        {
            var validationResult = validator.Validate(payload);

            if (!validationResult.IsValid)
            {
                extractedEvent = null;
                return false;
            }

            var eventData = ExtractionRules.Aggregate(new JObject(),
                (current, extractionRule) => extractionRule.ApplyRule(current, payload));

            extractedEvent = new Event
            {
                Data = eventData,
                Name = Event
            };

            return true;
        }
    }
}