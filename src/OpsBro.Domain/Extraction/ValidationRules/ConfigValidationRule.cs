using System;
using FluentValidation;
using Newtonsoft.Json.Linq;

namespace OpsBro.Domain.Extraction.ValidationRules
{
    /// <summary>
    /// Validation rule that will compare JToken found by <see cref="ValidationRule.Path"/> with JToken found in Config object with <see cref="ConfigPath"/>
    /// </summary>
    public class ConfigValidationRule : ValidationRule
    {
        /// <summary>
        /// This is a hotfix to simplify Config passing to ConfigValidationRule <br/>
        /// Suggestions are welcome to reduce this static field usage
        /// </summary>
        public static JObject Config { get; internal set; }

        public ConfigValidationRule(string path, ValidationOperator @operator, string configPath) : base(path, @operator)
        {
            ConfigPath = configPath ?? throw new ArgumentNullException(nameof(configPath));
        }

        public string ConfigPath { get; }
        public override InlineValidator<JObject> Populate(InlineValidator<JObject> validator)
        {
            switch (Operator)
            {
                case ValidationOperator.Equals:
                    validator.RuleFor(o => o.SelectToken(Path))
                        .Must(token =>
                        {
                            var comparisonValue = Config.SelectToken(ConfigPath);
                            var tokensAreEqual = JToken.DeepEquals(token, comparisonValue);

                            return tokensAreEqual;
                        })
                        .WithName(Path);
                    break;
                case ValidationOperator.NotEquals:
                    validator.RuleFor(o => o.SelectToken(Path))
                        .Must(token =>
                        {
                            var comparisonValue = Config.SelectToken(ConfigPath);
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