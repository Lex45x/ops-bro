using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpsBro.Api.Swagger
{
    public class PrometheusMetricsDocumentFilter : IDocumentFilter
    {
        private OpenApiPathItem PrometheusMetricsPath
        {
            get
            {
                var pathItem = new OpenApiPathItem();
                pathItem.Operations.Add(OperationType.Get, new OpenApiOperation
                {
                    Tags = new List<OpenApiTag> {
                        new OpenApiTag() {
                            Name = "Prometheus"
                        }
                    },
                    OperationId = "metrics",
                    Parameters = null,
                    Responses = new OpenApiResponses()
                    {
                        ["200"] = new OpenApiResponse()
                    }
                });

                return pathItem;
            }
        }
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            swaggerDoc.Paths.Add("/metrics", PrometheusMetricsPath);
        }
    }
}
