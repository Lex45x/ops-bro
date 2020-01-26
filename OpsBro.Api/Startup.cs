using OpsBro.Api.Swagger;
using OpsBro.Domain;

namespace OpsBro.Api
{
    public class Startup
    {
        private readonly IHostingEnvironment environment;

        public Startup(IHostingEnvironment environment)
        {
            this.environment = environment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options =>
            {
                options.Filters.Add<ExceptionFilter>();
                options.Filters.Add<RequestLoggingFilter>();

            }).AddJsonOptions(options =>
            {
                options.SerializerSettings.Converters.Add(new StringEnumConverter());
            });

            services.AddCors(options =>
                options.AddPolicy("AllowAll",
                    policyBuilder => policyBuilder
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowAnyOrigin()
                        .AllowCredentials()));

            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add(new CorsAuthorizationFilterFactory("AllowAll"));
            });

            services.AddSwaggerGen(options =>
            {
                options.CustomSchemaIds(type => type.FriendlyId(true));

                options.TagActionsBy(description =>
                    description.ControllerAttributes()
                        .OfType<ControllerNameAttribute>()
                        .FirstOrDefault()?.Name);
                
                options.SchemaFilter<ReadonlyFilter>();

                options.DescribeAllEnumsAsStrings();

                options.OrderActionsBy(description =>
                    description.ControllerAttributes()
                        .OfType<ControllerNameAttribute>()
                        .FirstOrDefault()
                        ?.Name);

                options.SwaggerDoc("v1", new Info { Title = "PowerBinder API", Version = "v1" });
            });


            var builder = new ContainerBuilder();

            builder.RegisterInstance(environment)
                .ExternallyOwned();

            builder.Populate(services);

            builder.RegisterModule(new ConfigurationModule(Assembly.GetExecutingAssembly()));
            builder.RegisterModule<Log4NetModule>();
            builder.RegisterModule<LoggingMiddlewareModule>();
            builder.RegisterModule<ExceptionsModule>();
            builder.RegisterModule<DomainModule>();
            builder.RegisterModule<AutofacApplicationModule>();
            builder.RegisterModule<CryptographyModule>();
            builder.RegisterModule<ReCaptchaCoreModule>();
            builder.RegisterType<HttpClient>()
                .AsSelf();

            var context = builder.Build();
            
            return new AutofacServiceProvider(context);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseSwagger();

            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "ComfortCity.Api");
            });

            app.UseCors("AllowAll");

            app.UseMvc();
        }
    }
}
