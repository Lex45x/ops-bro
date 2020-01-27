namespace OpsBro.Domain.Extraction
{
    public class ValidationRule
    {
        public ValidationRule(string path, object value, ValidationOperator @operator)
        {
            Operator = @operator;
            Path = path;
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