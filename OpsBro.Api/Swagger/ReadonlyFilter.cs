namespace OpsBro.Api.Swagger
{
    public class ReadonlyFilter : ISchemaFilter
    {
        public void Apply(Schema model, SchemaFilterContext context)
        {
            model.Properties = model.Properties?
                .Where(pair => !pair.Value.ReadOnly.HasValue || !pair.Value.ReadOnly.Value)
                .ToDictionary(pair => pair.Key, pair => pair.Value);
        }
    }
}