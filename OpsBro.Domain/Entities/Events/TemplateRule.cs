namespace OpsBro.Domain.Entities.Events
{
    /// <summary>
    /// Used to define template rules inside vent subscriber
    /// </summary>
    public class TemplateRule
    {
        /// <summary>
        /// Token to be found in template. It can be executed in differ depends on template type. <br/>
        /// Body - json path. <br/>
        /// Header - name of a header. <br/>
        /// Url - string to replace
        /// </summary>
        public string Token { get; }

        public TemplateRule(string token, string property)
        {
            Token = token;
            Property = property;
        }

        /// <summary>
        /// Path inside model JObject. Use `model.event` to access event and `model.meta` to access subscriber metadata.
        /// </summary>
        public string Property { get; }
    }
}