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
        public Extractor(string name, string eventName, ICollection<ExtractionRule> extractionRules,
            ICollection<ValidationRule> validationRules)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            EventName = eventName ?? throw new ArgumentNullException(nameof(eventName));
            ExtractionRules = extractionRules ?? throw new ArgumentNullException(nameof(extractionRules));
            ValidationRules = validationRules ?? throw new ArgumentNullException(nameof(validationRules));
            
            Validator = CreateValidator();
        }

        /// <summary>
        /// Creates an <see cref="IValidator{T}"/> implementation based on <see cref="ValidationRules"/>
        /// </summary>
        /// <returns></returns>
        private IValidator<JObject> CreateValidator()
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
                            })
                            .WithName(validationRule.Path);
                        break;
                    case ValidationOperator.NotEquals:
                        inlineValidator.RuleFor(o => o.SelectToken(validationRule.Path))
                            .Must(token =>
                            {
                                var comparisonValue = JToken.FromObject(validationRule.Value ?? JValue.CreateNull());
                                var tokensAreEqual = JToken.DeepEquals(token, comparisonValue);

                                return !tokensAreEqual;
                            })
                            .WithName(validationRule.Path);
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
        /// Name of Event that will be extracted as a result
        /// </summary>
        public string EventName { get; }

        /// <summary>
        /// Set of rules to extract an event
        /// </summary>
        public ICollection<ExtractionRule> ExtractionRules { get; }

        /// <summary>
        /// ValidationRules that help to ensure that event can be extracted
        /// </summary>
        public ICollection<ValidationRule> ValidationRules { get; }

        private IValidator<JObject> Validator { get; }

        /// <summary>
        /// Makes an attempt to extract event from payload
        /// </summary>
        /// <param name="payload">Payload to extract from</param>
        /// <param name="extractedEvent">Extracted event</param>
        /// <returns>Status of extraction. True when <paramref name="extractedEvent"/> created and initialized. </returns>
        public virtual bool TryExtract(JObject payload, [MaybeNullWhen(returnValue: false)] out Event extractedEvent)
        {
            if (payload == null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            var validationResult = Validator.Validate(payload);

            if (!validationResult.IsValid)
            {
                extractedEvent = null;
                return false;
            }

            var eventData = ExtractionRules.Aggregate(new JObject(),
                (current, extractionRule) => extractionRule.ApplyRule(current, payload));

            extractedEvent = new Event(EventName, eventData);

            return true;
        }
    }
}