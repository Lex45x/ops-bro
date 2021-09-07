using HandlebarsDotNet;
using Newtonsoft.Json.Linq;

namespace OpsBro.Domain.Events.Templates
{
    /// <summary>
    /// Provides template complication functionality with <see cref="Handlebars"/>
    /// </summary>
    public class HandlebarTemplate
    {
        private readonly HandlebarsTemplate<object, object> template;

        public HandlebarTemplate(string value)
        {
            Value = value;
            template = Handlebars.Compile(value);
        }

        /// <summary>
        /// Value of template that expected to be compiled.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Builds <see cref="Value"/> as <see cref="Handlebars"/> using <paramref name="model"/>
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public virtual string Build(JObject model)
        {
            return template(model);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Value;
        }

        public static implicit operator HandlebarTemplate(string value)
        {
            return new HandlebarTemplate(value);
        }
    }
}