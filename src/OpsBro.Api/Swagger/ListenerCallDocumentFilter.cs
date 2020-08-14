using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpsBro.Api.Swagger
{
    public class ListenerCallDocumentFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (context.MethodInfo.DeclaringType.Name == "ListenerController" && context.MethodInfo.Name == "Call")
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    In = ParameterLocation.Query,
                    Name = "query",
                    Schema = new OpenApiSchema { Type = "object" }
                });
            }
        }
    }
}
