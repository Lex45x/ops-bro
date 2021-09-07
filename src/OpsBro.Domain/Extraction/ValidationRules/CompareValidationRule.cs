using System;
using FluentValidation;
using Newtonsoft.Json.Linq;

namespace OpsBro.Domain.Extraction.ValidationRules
{
    /// <summary>
    /// Validation rule that will compare JToken found by <see cref="ValidationRule.Path"/> with JToken found in Config object with <see cref="WithPath"/>
    /// </summary>
    public class CompareValidationRule : ValidationRule
    {
        public CompareValidationRule(string path, ValidationOperator @operator, string withPath) : base(path, @operator)
        {
            WithPath = withPath ?? throw new ArgumentNullException(nameof(withPath));
        }

        public string WithPath { get; }
        public override InlineValidator<JObject> Populate(InlineValidator<JObject> validator)
        {
            switch (Operator)
            {
                case ValidationOperator.Equals:
                    validator.RuleFor(o => o)
                        .Must(@object =>
                        {
                            var value = @object.SelectToken(Path);
                            var comparisonValue = @object.SelectToken(WithPath);
                            var tokensAreEqual = JToken.DeepEquals(value, comparisonValue);

                            return tokensAreEqual;
                        })
                        .WithName(Path);
                    break;
                case ValidationOperator.NotEquals:
                    validator.RuleFor(o => o)
                        .Must(@object =>
                        {
                            var value = @object.SelectToken(Path);
                            var comparisonValue = @object.SelectToken(WithPath);
                            var tokensAreEqual = JToken.DeepEquals(value, comparisonValue);

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