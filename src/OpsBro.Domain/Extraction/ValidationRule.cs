using System;
using System.ComponentModel;

namespace OpsBro.Domain.Extraction
{
    public class ValidationRule
    {
        public ValidationRule(string path, object value, ValidationOperator @operator)
        {
            if (!Enum.IsDefined(typeof(ValidationOperator), @operator))
            {
                throw new InvalidEnumArgumentException(nameof(@operator), (int) @operator, typeof(ValidationOperator));
            }

            Operator = @operator;
            Path = path ?? throw new ArgumentNullException(nameof(path));
            Value = value;
        }

        /// <summary>
        /// Comparison operator
        /// </summary>
        public ValidationOperator Operator { get; }

        /// <summary>
        /// Json path in payload
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Value to compare with
        /// </summary>
        public object Value { get; }
    }
}