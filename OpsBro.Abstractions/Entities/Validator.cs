namespace OpsBro.Abstractions.Entities
{
    public class Validator
    {
        public Operator Operator { get; set; }
        public string Path { get; set; }
        public object Value { get; set; }
    }

    public enum Operator
    {
        Equals = 1 << 0,
        NotEquals = 1 << 1
    }
}