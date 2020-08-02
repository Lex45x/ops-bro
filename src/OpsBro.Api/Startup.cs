using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace OpsBro.Api
{
    public class Startup
    {
        private static readonly string _corsPolicyName = "AllowAll";

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvc()
                .AddNewtonsoftJson();

            services.AddCors(options =>
                options.AddPolicy(_corsPolicyName,
                    policyBuilder => policyBuilder
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowAnyOrigin()));

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v0.1", new OpenApiInfo
                {
                    Contact = new OpenApiContact
                    {
                        Name = "GitHub Repo",
                        Url = new Uri("https://github.com/Lex45x/ops-bro")
                    },
                    Version = "v0.1",
                    Title = "ops-bro"
                });
            });

            services.AddSingleton(Program.Settings);

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UseSwagger();

            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v0.1/swagger.json", "OpsBro.Api");
                options.RoutePrefix = string.Empty;
            });

            app.UseCors(_corsPolicyName);
            app.UseRouting();
            app.UseEndpoints(builder => builder.MapControllers());
        }
    }
}