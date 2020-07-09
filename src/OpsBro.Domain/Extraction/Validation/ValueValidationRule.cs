using System;
using FluentValidation;
using Newtonsoft.Json.Linq;

namespace OpsBro.Domain.Extraction.Validation
{
    /// <summary>
    /// A validation rule that will compare JToken found via <see cref="ValidationRule.Path"/> with <see cref="Value"/> 
    /// </summary>
    public class ValueValidationRule : ValidationRule
    {
        public ValueValidationRule(string path, object value, ValidationOperator @operator) : base(path, @operator)
        {
            Value = value;
        }

        /// <summary>
        /// Value to compare with
        /// </summary>
        public object Value { get; }

        public override InlineValidator<JObject> Populate(InlineValidator<JObject> validator)
        {
            switch (Operator)
            {
                case ValidationOperator.Equals:
                    validator.RuleFor(o => o.SelectToken(Path))
                        .Must(token =>
                        {
                            var comparisonValue = JToken.FromObject(Value ?? JValue.CreateNull());
                            var tokensAreEqual = JToken.DeepEquals(token, comparisonValue);

                            return tokensAreEqual;
                        })
                        .WithName(Path);
                    break;
                case ValidationOperator.NotEquals:
                    validator.RuleFor(o => o.SelectToken(Path))
                        .Must(token =>
                        {
                            var comparisonValue = JToken.FromObject(Value ?? JValue.CreateNull());
                            var tokensAreEqual = JToken.DeepEquals(token, comparisonValue);

                            return !tokensAreEqual;
                        })
                        .WithName(Path);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return validator;
        }
    }
}