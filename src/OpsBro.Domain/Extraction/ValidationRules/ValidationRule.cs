using System;
using System.ComponentModel;
using FluentValidation;
using Newtonsoft.Json.Linq;

namespace OpsBro.Domain.Extraction.ValidationRules
{
    /// <summary>
    /// Represent a rule to validate JToken that may be found by <see cref="Path"/>
    /// </summary>
    public abstract class ValidationRule
    {
        protected ValidationRule(string path, ValidationOperator @operator)
        {
            if (!Enum.IsDefined(typeof(ValidationOperator), @operator))
            {
                throw new InvalidEnumArgumentException(nameof(@operator), (int) @operator, typeof(ValidationOperator));
            }

            Operator = @operator;
            Path = path ?? throw new ArgumentNullException(nameof(path));
        }

        /// <summary>
        /// Comparison operator
        /// </summary>
        public ValidationOperator Operator { get; }

        /// <summary>
        /// Json path in payload
        /// </summary>
        public string Path { get; }

        public abstract InlineValidator<JObject> Populate(InlineValidator<JObject> validator);
    }
}