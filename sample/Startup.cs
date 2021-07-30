using CommandQuery.Framing;
using CommandQueryApiSample.Domain.Commands;
using CommandQueryApiSample.Domain.MessageHandlers;
using CommandQueryApiSample.Domain.Messages;
using CommandQueryApiSample.Domain.Models;
using CommandQueryApiSample.Domain.Queries;
using CommandQueryApiSample.Domain.Requests;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using CreateWidget = CommandQueryApiSample.Domain.Commands.CreateWidget;

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
            services.AddSingleton<ICommandBroker, CommandBroker>();
            services.AddSingleton<IDomainEventPublisher, DomainEventPublisher>();
            services.AddTransient<IAsyncHandler<Domain.Requests.CreateWidget, CommandResponse>, CreateWidget>();
            services.AddTransient<IAsyncQueryHandler<GetWidget, Widget>, GetWidgetQuery>();
            services.AddTransient<IDomainEvent<WidgetCreated>, WidgetCreatedHandler>();

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