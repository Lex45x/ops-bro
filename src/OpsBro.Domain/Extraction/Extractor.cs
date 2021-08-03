using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using FluentValidation;
using Newtonsoft.Json.Linq;
using NLog;
using OpsBro.Domain.Events;
using OpsBro.Domain.Extraction.ExtractionRules;
using OpsBro.Domain.Extraction.ValidationRules;

namespace OpsBro.Domain.Extraction
{
    /// <summary>
    /// Define rules to extract an event from payload
    /// </summary>
    public class Extractor
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();
        public Extractor(string comment, string eventName, ICollection<ExtractionRule> extractionRules,
            ICollection<ValidationRule> validationRules)
        {
            Comment = comment ?? throw new ArgumentNullException(nameof(comment));
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

            return ValidationRules.Aggregate(inlineValidator,
                (current, validationRule) => validationRule.Populate(current));
        }

        /// <summary>
        /// Description of the specific extractor
        /// </summary>
        public string Comment { get; }

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
                logger.Debug("Event extraction interrupted with validation failure. {failure}", validationResult.Errors.Aggregate(
                    new StringBuilder(),
                    (builder, failure) => builder.AppendLine(failure.ErrorMessage)));

                extractedEvent = null;
                return false;
            }

            var eventData = ExtractionRules.Aggregate(new JObject(),
                (current, extractionRule) => extractionRule.ApplyRule(current, payload));

            extractedEvent = new Event(EventName, eventData);

            logger.Debug("Event extracted! {event}", extractedEvent);

            return true;
        }
    }
}