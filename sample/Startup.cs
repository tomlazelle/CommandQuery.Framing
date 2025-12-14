using CommandQuery.Framing;
using CommandQueryApiSample.Domain.Messages;
using CommandQueryApiSample.Domain.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace CommandQueryApiSample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Register CommandQuery framework
            services
                .AddCommandQuery(typeof(Startup).Assembly);
            
            // Configure domain event pipeline for WidgetCreated messages
            // This adds middleware that runs before and after domain event handlers
            services
                .AddDomainEventMiddleware<DomainEventLoggingMiddleware<WidgetCreated>>()
                .AddDomainEventMiddleware<DomainEventValidationMiddleware<WidgetCreated>>()
                .AddDomainEventPipeline<WidgetCreated>(builder =>
                {
                    // Validation runs first
                    builder.Use<DomainEventValidationMiddleware<WidgetCreated>>();
                    // Then logging wraps the actual handler execution
                    builder.Use<DomainEventLoggingMiddleware<WidgetCreated>>();
                });

            services
                .AddControllers()   
                .AddNewtonsoftJson(x => new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}